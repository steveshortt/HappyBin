using System;
using System.ComponentModel;
using System.Threading.Tasks;

using HappyBin.AutoUpdater;

namespace HappyBinCli
{
    class Program
    {
        static void Main(string[] args)
        {
            new UpdaterWrapper().RunUpdate();
        }
    }

    class UpdaterWrapper
    {
        Updater _updater = new Updater();
        bool _isWorking = false;

        public async void RunUpdate()
        {
            _updater.PropertyChanged += new PropertyChangedEventHandler( updater_PropertyChanged );
            _updater.InitializePatchStatus();

            if( _updater.Status.PatchIsValid )
                if( _updater.Status.PatchIsMandatory )
                {
                    _isWorking = true;
                    await InstallPatchesAsync();
                }
                else
                {
                    Console.Write( "An update is available and ready to install.  Would you like to install now? " );
                    char response = Convert.ToChar( Console.Read() );
                    if( response == 'y' )
                    {
                        _isWorking = true;
                        await InstallPatchesAsync();
                    }
                }
            else
                _updater = null;

            while( _isWorking ) { System.Threading.Thread.Sleep( 100 ); }

            Console.WriteLine( "done" );
        }

        Task InstallPatchesAsync()
        {
            _updater.InstallExistingPatches( _updater.Status.ExeInfo.Name, _updater.Status.ExeInfo.FolderPath );
            _isWorking = false;
            return null;
        }

        void updater_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if( e.PropertyName == "LogMessage" )
                Console.WriteLine( "{0}\t{1}", _updater.LogMessage.TimeStamp, _updater.LogMessage.Message );
        }
    }
}