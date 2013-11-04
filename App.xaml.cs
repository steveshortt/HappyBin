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
		public static Updater Updater { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup( e );

			Updater = new Updater();
			MainDlg mainDlg = new MainDlg();

			Updater.IsAboutBox = false;
			if( e.Args.Length > 0 )
			{
				Updater.IsAboutBox = e.Args[0].ToLower() == "/about";
			}

			if( !Updater.IsAboutBox )
			{
				Updater.InitializePatchStatus();

				if( Updater.Status.PatchFilePathExists )
				{
					mainDlg.Show();
				}
				else
				{
					Updater = null;
					App.Current.Shutdown();
				}
			}
			else
			{
				mainDlg.Show();
			}
		}
	}
}