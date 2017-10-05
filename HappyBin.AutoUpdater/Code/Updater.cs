using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Xml;

//using ICSharpCode.SharpZipLib.Zip;


namespace HappyBin.AutoUpdater
{
    public class Updater : INotifyPropertyChanged
    {
        const string __uriSchemeHttp = "http";
        const string __uriSchemeFtp = "ftp";
        const string __purgeListFileName = "purge.txt";

        #region friendly ui stuff
        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs<string>> DownloadCompleted;
        public event EventHandler<EventArgs<string>> UnzipFileCompleted;
        public event EventHandler<EventArgs<string>> UnzipCompleted;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected virtual void OnDownloadCompleted(string fileName)
        {
            DownloadCompleted?.Invoke( this, new EventArgs<string>( fileName ) );
        }

        protected virtual void OnUnzipFileCompleted(string fileName)
        {
            UnzipFileCompleted?.Invoke( this, new EventArgs<string>( fileName ) );
        }

        protected virtual void OnUnzipCompleted(string fileName)
        {
            UnzipCompleted?.Invoke( this, new EventArgs<string>( fileName ) );
        }
        #endregion

        #region public properties
        PatchStatus _status = new PatchStatus();
        string _downloadFileName = string.Empty;
        int _downloadBytes = 0;
        LogMessage _logMessage = new LogMessage();
        bool _isAboutBox = false;

        public PatchStatus Status
        {
            get { return _status; }
            private set
            {
                if( _status != value )
                {
                    _status = value;
                    OnPropertyChanged( "PatchStatus" );
                }
            }
        }

        public string DownloadFileName
        {
            get { return _downloadFileName; }
            private set
            {
                if( _downloadFileName != value )
                {
                    _downloadFileName = value;
                    OnPropertyChanged( "DownloadFileName" );
                }
            }
        }

        public int DownloadBytes
        {
            get { return _downloadBytes; }
            private set
            {
                if( _downloadBytes != value )
                {
                    _downloadBytes = value;
                    OnPropertyChanged( "DownloadBytes" );
                }
            }
        }

        public LogMessage LogMessage
        {
            get { return _logMessage; }
            private set
            {
                if( _logMessage != value )
                {
                    _logMessage = value;

                    OnPropertyChanged( "LogMessage" );
                }
            }
        }

        //hack: this has nothing to do with the updater.  it's just for the ui.
        public bool IsAboutBox
        {
            get { return _isAboutBox; }
            set
            {
                if( _isAboutBox != value )
                {
                    _isAboutBox = value;

                    OnPropertyChanged( "IsAboutBox" );
                }
            }
        }

        #endregion
        #endregion

        #region public methods
        public void CompareDownloadInstall()
        {
            PatchStatus result = InitializePatchStatus();

            //run it if required
            if( result.PatchIsValid && result.PatchIsMandatory )
            {
                InstallExistingPatches( result.ExeInfo.FullPath, result.ExeInfo.FolderPath );
            }
        }

        /// <summary>
        /// Downloads config from specified Uri and compares version to runtime exe version.  Starts update if update is marked as 'Mandatory'.
        /// </summary>
        public PatchStatus InitializePatchStatus()
        {
            PatchStatus result = new PatchStatus();
            Status = result;

            //assemble accurate path and file names, check for exe existence, bail if not present
            RuntimeExeInfo rei = new RuntimeExeInfo( Properties.Settings.Default.RuntimeExe );
            result.ExeInfo = rei;

            if( !rei.Exists )
            {
                SetLogMessage( "Could not find file {0}; aborting.", true, rei.FullPath );
                return result;
            }

            //this will always return a valid config, but it will be empty on load failure (CurrVer = 0.0.0.0)
            UpdateConfig uc = LoadConfig();
            result.PatchIsMandatory = uc.IsMandatory;

            if( uc.CurrentVer > rei.Version )
            {
                SetLogMessage( "Current version is {0}, getting new version: {1}", rei.Version, uc.CurrentVersion );

                if( uc.LastMandatoryVer > rei.Version )
                {
                    result.PatchIsMandatory = true;
                    SetLogMessage( "Current patch is Mandatory due to version age: {0} / {1}", rei.Version, uc.LastMandatoryVer );
                }

                #region ensure patch folders exist
                string downloadFolderRoot = Properties.Settings.Default.DownloadFolder;
                if( !Directory.Exists( downloadFolderRoot ) )
                {
                    Directory.CreateDirectory( downloadFolderRoot );
                }

                Uri patchUri = new Uri( uc.PatchUri );
                string fileName = patchUri.Segments[patchUri.Segments.Length - 1];

                string patchFolder = Path.Combine( downloadFolderRoot, Path.GetFileNameWithoutExtension( fileName ) );
                if( !Directory.Exists( patchFolder ) )
                {
                    Directory.CreateDirectory( patchFolder );
                }
                #endregion

                //download/copy the patch file
                string patchFilePath = Path.Combine( patchFolder, fileName );
                result.PatchIsValid = PatchFileIsValid( patchFilePath, uc.PatchSizeBytes );
                if( !result.PatchIsValid )
                {
                    result.PatchIsValid = GetPatchFile( patchUri, patchFilePath, uc.PatchSizeBytes );
                }
            }

            return result;
        }

        /// <summary>
        /// Load the configuration file specified in this exe's config "UpdateConfigUri" setting.
        /// </summary>
        /// <returns>The initialized UpdateConfig.  Values will be empty if the a load failure occurs.</returns>
        public UpdateConfig LoadConfig()
        {
            Uri configUri = new Uri( Properties.Settings.Default.UpdateConfigUri );
            UpdateConfig uc = new UpdateConfig();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                if( configUri.IsUnc )
                {
                    SetLogMessage( "Getting config via unc from {0}", configUri.LocalPath );
                    xmlDoc.Load( configUri.LocalPath );
                }
                else if( configUri.Scheme == __uriSchemeHttp )
                {
                    SetLogMessage( "Getting config via http from {0}", configUri.AbsoluteUri );
                    Stream respStream = HttpRequestSync( configUri.AbsoluteUri );
                    if( respStream != null )
                    {
                        xmlDoc.Load( respStream );
                        respStream.Close();
                    }
                }
                else if( configUri.Scheme == __uriSchemeFtp )
                {
                    SetLogMessage( "Getting file via ftp from {0}", configUri.AbsoluteUri );
                    Stream respStream = FtpRequestSync( configUri.AbsoluteUri );
                    if( respStream != null )
                    {
                        xmlDoc.Load( respStream );
                        respStream.Close();
                    }
                }
                else
                {
                    throw new UriFormatException( "Unknown Uri format" );
                }

                uc = UpdateConfig.Deserialize( new StringReader( xmlDoc.OuterXml ) );
            }
            catch( Exception ex )
            {
                SetLogMessage( "Failed to retrieve config file: {0}.", ex, configUri );
            }

            return uc;
        }

        /// <summary>
        /// Helper method for InstallExistingPatches(string processName, string rootUnzipPath) that uses default pathing
        /// </summary>
        public void InstallExistingPatches()
        {
            //assemble accurate path and file names, check for exe existence, bail if not present
            RuntimeExeInfo rei = new RuntimeExeInfo( Properties.Settings.Default.RuntimeExe );

            if( !rei.Exists )
            {
                SetLogMessage( "Could not find file {0}; aborting.", true, rei.FullPath );
            }
            else
            {
                SetLogMessage( "Starting installation of existing patches." );
                InstallExistingPatches( rei.FullPath, rei.FolderPath );
            }
        }

        /// <summary>
        /// Clears existing instances of the application from memory and installs patches in order of folder CreationTime
        /// </summary>
        /// <param name="processName">Name of exe to Kill</param>
        /// <param name="rootUnzipPath">Root path where files will be unzipped</param>
        public void InstallExistingPatches(string processName, string rootUnzipPath)
        {
            int waitMilliseconds = Properties.Settings.Default.WaitForExitMillseconds;

            Process[] currExe = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( processName ) );
            foreach( Process exe in currExe )
            {
                exe.CloseMainWindow();
                bool exited = exe.WaitForExit( waitMilliseconds );
                if( !exited )
                {
                    SetLogMessage( "Killing process: {0} PId:({1})", exe.ProcessName, exe.Id );
                    exe.Kill();
                }
            }

            DirectoryInfo downloadFolder = new DirectoryInfo( Properties.Settings.Default.DownloadFolder );
            List<DirectoryInfo> patchFolders = new List<DirectoryInfo>( downloadFolder.EnumerateDirectories() );
            patchFolders.Sort( CompareDirectoriesByCreationTime );

            foreach( DirectoryInfo patchFolder in patchFolders )
            {
                foreach( FileInfo file in patchFolder.EnumerateFiles( "*.zip" ) )
                {
                    string patchPath = file.FullName;
                    SetLogMessage( "Starting patch installation for {0}", patchPath );
                    UnzipFile( patchPath, rootUnzipPath );

                    string purgeListFile = Path.Combine( Path.GetDirectoryName( patchPath ), __purgeListFileName );
                    if( File.Exists( purgeListFile ) )
                    {
                        IEnumerable<string> purgelist = File.ReadAllLines( purgeListFile );
                        foreach( string purgefile in purgelist )
                        {
                            FileInfo p = new FileInfo( Path.Combine( rootUnzipPath, purgefile ) );
                            if( p.Exists )
                            {
                                TryDeleteFile( p );
                            }
                        }
                        TryDeleteFile( new FileInfo( purgeListFile ) );
                    }

                    //cleanup
                    TryDeleteFile( file );
                    TryDeleteDirectory( patchFolder );
                }
            }

            //cleanup
            TryDeleteDirectory( downloadFolder );

            if( Properties.Settings.Default.StartRuntimeExeAfterInstall )
                Process.Start( processName );
        }
        #endregion


        #region private methods
        /// <summary>
        /// Downloads or copies the patch file from web or unc
        /// </summary>
        /// <param name="patchUri">Patch file source Uri</param>
        /// <param name="destination">Patch file local destination folder/file name</param>
        /// <returns></returns>
        private bool GetPatchFile(Uri patchUri, string destination, long patchSizeBytes)
        {
            try
            {
                if( patchUri.IsUnc )
                {
                    SetLogMessage( "Getting file via unc from {0}", patchUri.LocalPath );
                    File.Copy( patchUri.LocalPath, destination );
                }
                else if( patchUri.Scheme == __uriSchemeHttp )
                {
                    SetLogMessage( "Getting file via http from {0}", patchUri.AbsoluteUri );
                    Stream respStream = HttpRequestSync( patchUri.AbsoluteUri );
                    if( respStream != null )
                    {
                        WriteFile( respStream, destination );
                        respStream.Close();
                    }
                }
                else if( patchUri.Scheme == __uriSchemeFtp )
                {
                    SetLogMessage( "Getting file via ftp from {0}", patchUri.AbsoluteUri );
                    Stream respStream = FtpRequestSync( patchUri.AbsoluteUri );
                    if( respStream != null )
                    {
                        WriteFile( respStream, destination );
                        respStream.Close();
                    }
                }
                else
                {
                    throw new UriFormatException( "Unknown Uri format" );
                }
            }
            catch( Exception ex )
            {
                SetLogMessage( "Failed to retrieve config file: {0}.", ex, patchUri );
            }

            return PatchFileIsValid( destination, patchSizeBytes );
        }

        private bool PatchFileIsValid(string patchFilePath, long patchSizeBytes)
        {
            FileInfo fi = new FileInfo( patchFilePath );
            bool isValid = fi.Exists && fi.Length == patchSizeBytes;
            if( !isValid )
            {
                TryDeleteFile( fi );
            }

            return isValid;
        }

        /// <summary>
        /// Writes the contents of a stream to a file 
        /// </summary>
        /// <param name="s">The source stream</param>
        /// <param name="outPath">Output file name</param>
        private void WriteFile(Stream s, string outPath)
        {
            DownloadBytes = 0;
            byte[] buf = new byte[4096];
            int length;

            using( FileStream fs = File.Open( outPath, FileMode.Create, FileAccess.Write ) )
            {
                length = s.Read( buf, 0, 4096 );
                while( length > 0 )
                {
                    DownloadBytes += length;

                    fs.Write( buf, 0, length );
                    length = s.Read( buf, 0, 4096 );
                }
            }
        }

        private Stream HttpRequestSync(string uri)
        {
            try
            {
                DownloadFileName = uri;
                SetLogMessage( "Downloading file via http: {0}", uri );

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create( uri );
                req.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                OnDownloadCompleted( uri );
                SetLogMessage( "Download file via http  complete: {0}", uri );

                return resp.GetResponseStream();
            }
            catch( Exception ex )
            {
                SetLogMessage( "Failed on http call to: {0}.", ex, uri );
                return null;
            }
        }

        private Stream FtpRequestSync(string uri)
        {
            try
            {
                DownloadFileName = uri;
                SetLogMessage( "Downloading file via ftp: {0}", uri );

                FtpWebRequest req = (FtpWebRequest)WebRequest.Create( uri );
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.Credentials = CredentialCache.DefaultCredentials;
                FtpWebResponse resp = (FtpWebResponse)req.GetResponse();

                OnDownloadCompleted( uri );
                SetLogMessage( "Download file via ftp complete: {0}", uri );

                return resp.GetResponseStream();
            }
            catch( Exception ex )
            {
                SetLogMessage( "Failed on ftp call to: {0}.", ex, uri );
                return null;
            }
        }

        private void UnzipFile(string zipfile, string rootUnzipPath)
        {
            using( ZipArchive archive = ZipFile.OpenRead( zipfile ) )
            {
                foreach( ZipArchiveEntry entry in archive.Entries )
                {
                    string directoryName = Path.Combine( rootUnzipPath, Path.GetDirectoryName( entry.FullName ) );
                    string fileName = Path.GetFileName( entry.Name );

                    // create directory
                    if( directoryName.Length > 0 )
                        Directory.CreateDirectory( directoryName );

                    if( fileName != String.Empty )
                    {
                        string fullFilePath = Path.Combine( rootUnzipPath, entry.Name );

                        if( entry.Name == __purgeListFileName )
                            fullFilePath = Path.Combine( Path.GetDirectoryName( zipfile ), entry.Name );

                        entry.ExtractToFile( Path.Combine( rootUnzipPath, entry.FullName ) );

                        OnUnzipFileCompleted( fullFilePath );
                        SetLogMessage( "Unzipped file {0}", fullFilePath );
                    }
                }
            }

            OnUnzipFileCompleted( zipfile );
            SetLogMessage( "Completed unzip for {0}", zipfile );
        }

        private int CompareDirectoriesByCreationTime(DirectoryInfo x, DirectoryInfo y)
        {
            return x.CreationTime.CompareTo( y.CreationTime );
        }

        private void TryDeleteFile(FileInfo file)
        {
            try
            {
                SetLogMessage( "Removing file {0}", file.FullName );
                file.Delete();
            }
            catch( Exception ex )
            {
                SetLogMessage( "Could not remove file {0}.", ex, file.FullName );
            }
        }

        private void TryDeleteDirectory(DirectoryInfo dir)
        {
            try
            {
                SetLogMessage( "Removing folder {0}", dir.FullName );
                dir.Delete();
            }
            catch( Exception ex )
            {
                SetLogMessage( "Could not remove folder {0}.", ex, dir.FullName );
            }
        }

        private void SetLogMessage(string format, params object[] args)
        {
            LogMessage = new LogMessage( format, args );
        }

        private void SetLogMessage(string format, bool isError, params object[] args)
        {
            LogMessage l = new LogMessage( format, args );
            l.IsError = isError;
            LogMessage = l;
        }

        private void SetLogMessage(string format, Exception ex, params object[] args)
        {
            LogMessage = new LogMessage( format, ex, args );
        }
        #endregion
    }
}