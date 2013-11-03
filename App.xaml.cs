using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace HappyBin.AutoUpdater
{
	public partial class App : Application
	{
		public Updater Updater { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup( e );

			this.Updater = new Updater();
			MainDlg mainDlg = new MainDlg( this.Updater );

			this.Updater.InitializePatchStatus();

			if( this.Updater.Status.PatchFilePathExists )
			{
				mainDlg.Show();
			}
			else
			{
				this.Updater = null;
				App.Current.Shutdown();
			}
		}
	}
}