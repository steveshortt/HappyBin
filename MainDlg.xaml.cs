using System;
using System.Windows;
using System.ComponentModel;

namespace HappyBin.AutoUpdater
{
	public partial class MainDlg : Window
	{
		Updater _updater = null;

		public MainDlg(Updater updater)
		{
			InitializeComponent();

			_updater = updater;
			_updater.PropertyChanged += updater_PropertyChanged;
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

		void updater_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "LogMessage" )
			{
				App.Current.Dispatcher.Invoke( (Action)delegate()
				{
					txtLog.AppendText( string.Format( "{0}\t{1}\r\n", _updater.LogMessage.TimeStamp, _updater.LogMessage.Message ) );
				} );
			}
		}

		private void cmdNow_Click(object sender, RoutedEventArgs e)
		{
			this.InstallPatchesAsync();
		}

		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}
	}
}