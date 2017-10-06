using System;
using System.ComponentModel;
using System.Threading.Tasks;

using HappyBin.AutoUpdater;

using settings = HappyBinCli.Properties.Settings;

namespace HappyBinCli
{
    class Program
    {
        static Updater _updater = null;

        static void Main(string[] args)
        {
            _updater = new Updater( new UpdaterSettings()
            {
                DownloadFolder = settings.Default.DownloadFolder,
                RuntimeExe = settings.Default.ProcessName,
                ServiceName = settings.Default.ServiceName,
                StartProcessAfterInstall = settings.Default.StartProcessAfterInstall,
                UpdateConfigUri = settings.Default.UpdateConfigUri,
                WaitForExitMillseconds = settings.Default.WaitForExitMillseconds
            } );
            _updater.PropertyChanged += new PropertyChangedEventHandler( updater_PropertyChanged );

            _updater.InitializePatchStatus();

            bool runUpdate = false;
            if( _updater.Status.PatchIsValid )
                if( !_updater.Status.PatchIsMandatory )
                {
                    Console.Write( "An update is available and ready to install.  Would you like to install now? " );
                    char response = Convert.ToChar( Console.Read() );
                    runUpdate = response == 'y';
                }
                else
                    runUpdate = true;

            if( runUpdate )
                Task.Run( () =>
                {
                    _updater.InstallExistingPatches( _updater.Status.ExeInfo.Name, _updater.Status.ExeInfo.FolderPath );
                } ).Wait();


            Console.WriteLine( "Update complete." );
        }

        static void updater_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if( e.PropertyName == "LogMessage" )
                Console.WriteLine( "{0}\t{1}", _updater.LogMessage.TimeStamp, _updater.LogMessage.Message );
        }
    }
}