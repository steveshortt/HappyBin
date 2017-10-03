using System;
using System.ComponentModel;
using HappyBin.AutoUpdater;

namespace HappyBinCli
{
	class Program
	{
		static Updater _updater = new Updater();
		static bool _isWorking = false;

		static void Main(string[] args)
		{
			_updater.PropertyChanged += new PropertyChangedEventHandler( updater_PropertyChanged );
			_updater.InitializePatchStatus();

			if( _updater.Status.PatchIsValid )
			{
				if( _updater.Status.PatchIsMandatory )
				{
					_isWorking = true;
					InstallPatchesAsync();
				}
				else
				{
					Console.Write( "An update is available and ready to install.  Would you like to install now? " );
					char response = Convert.ToChar( Console.Read() );
					if( response == 'y' )
					{
						_isWorking = true;
						InstallPatchesAsync();
					}
				}
			}
			else
			{
				_updater = null;
			}

			while( _isWorking )
			{
				System.Threading.Thread.Sleep( 100 );
			}
		}

		static void InstallPatchesAsync()
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (s, v) =>
			{
				_updater.InstallExistingPatches( _updater.Status.ExeInfo.Name, _updater.Status.ExeInfo.FolderPath );
			};

			w.RunWorkerCompleted += (s, v) =>
			{
				_isWorking = false;
			};

			w.RunWorkerAsync();
		}

		static void updater_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "LogMessage" )
			{
				Console.WriteLine( "{0}\t{1}\r\n", _updater.LogMessage.TimeStamp, _updater.LogMessage.Message );
			}
		}
	}
}
