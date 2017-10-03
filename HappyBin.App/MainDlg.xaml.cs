using System;
using System.Windows;
using System.ComponentModel;
using log4net;

namespace HappyBin.AutoUpdater
{
	public partial class MainDlg : Window
	{
		Updater _updater = null;
		static readonly ILog _log = LogManager.GetLogger( "HappyBinLog" );

		public MainDlg()
		{
			InitializeComponent();

			_updater = App.Updater;
			_updater.PropertyChanged += Updater_PropertyChanged;

			this.DataContext = _updater;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if( _updater.Status.PatchIsMandatory )
			{
				this.InstallPatchesAsync();
			}
		}

		void InstallPatchesAsync()
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (s, v) =>
			{
				_updater.InstallExistingPatches( _updater.Status.ExeInfo.Name, _updater.Status.ExeInfo.FolderPath );
			};

			w.RunWorkerCompleted += (s, v) =>
			{
				System.Threading.Thread.Sleep( 3000 );
				App.Current.Shutdown();
			};

			w.RunWorkerAsync();
		}

		void Updater_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "LogMessage" )
			{
				App.Current.Dispatcher.Invoke( (Action)delegate()
				{
					if( _updater.LogMessage.IsError )
					{
						txtLog.AppendText(
							string.Format( "{0}\t{1}\r\n{2}\r\n",
							_updater.LogMessage.TimeStamp, _updater.LogMessage.Message, _updater.LogMessage.Exception ) );

						if( _log.IsErrorEnabled )
						{
							_log.Error( _updater.LogMessage.Message, _updater.LogMessage.Exception );
						}
					}
					else
					{
						txtLog.AppendText( string.Format( "{0}\t{1}\r\n", _updater.LogMessage.TimeStamp, _updater.LogMessage.Message ) );

						if( _log.IsInfoEnabled )
						{
							_log.Info( _updater.LogMessage.Message );
						}
					}
				} );
			}
			else if( e.PropertyName == "DownloadBytes" )
			{
				App.Current.Dispatcher.Invoke( (Action)delegate()
				{
					txtBytes.Text = _updater.DownloadBytes.ToString();
				} );
			}
		}

		private void CmdNow_Click(object sender, RoutedEventArgs e)
		{
			this.InstallPatchesAsync();
		}

		private void CmdCancel_Click(object sender, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}


		private void CmdCheckForUpdates_Click(object sender, RoutedEventArgs e)
		{
			this.InitializePatchStatusAsync();
		}

		void InitializePatchStatusAsync()
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (s, v) =>
			{
				_updater.InitializePatchStatus();
			};

			w.RunWorkerCompleted += (s, v) =>
			{
				if( _updater.Status.PatchIsValid )
				{
					_updater.IsAboutBox = false;

					if( _updater.Status.PatchIsMandatory )
					{
						this.InstallPatchesAsync();
					}
				}
				else
				{
					txtBytes.Text = "No updates availble.";
				}
			};

			w.RunWorkerAsync();
		}

		public void InstallExistingPatchesAsync()
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (s, v) =>
			{
				_updater.InstallExistingPatches();
			};

			w.RunWorkerCompleted += (s, v) =>
			{
				System.Threading.Thread.Sleep( 3000 );
				App.Current.Shutdown();
			};

			pnlButtons.Visibility = Visibility.Collapsed;
			txtMsg.Text = "Installing existing patches.";
			w.RunWorkerAsync();
		}

	}
}