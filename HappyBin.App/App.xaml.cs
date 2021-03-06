﻿using System;
using System.Windows;
using settings = HappyBin.AutoUpdater.Properties.Settings;

namespace HappyBin.AutoUpdater
{
    public partial class App : Application
    {
        MainDlg _mainDlg = null;

        public static Updater Updater { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup( e );

            Updater = new Updater( new UpdaterSettings()
            {
                DownloadFolder = settings.Default.DownloadFolder,
                RuntimeExe = settings.Default.RuntimeExe,
                ServiceName = settings.Default.ServiceName,
                StartProcessAfterInstall = settings.Default.StartProcessAfterInstall,
                UpdateConfigUri = settings.Default.UpdateConfigUri,
                WaitForExitMillseconds = settings.Default.WaitForExitMillseconds
            } );

            _mainDlg = new MainDlg();

            Updater.IsAboutBox = false;  //just being explicit
            if( e.Args.Length > 0 )
            {
                string arg = e.Args[0].ToLower();

                if( arg == "/install" )
                {
                    this.InstallPatchesAndExit();
                }
                else
                {
                    Updater.IsAboutBox = arg == "/about";
                }
            }

            if( Updater.IsAboutBox )
            {
                _mainDlg.Show();
            }
            else
            {
                this.CheckForPatches();
            }
        }

        private void InstallPatchesAndExit()
        {
            _mainDlg.Show();
            _mainDlg.InstallExistingPatchesAsync();
        }

        private void CheckForPatches()
        {
            Updater.InitializePatchStatus();

            if( Updater.Status.PatchIsValid )
            {
                _mainDlg.Show();
            }
            else
            {
                Updater = null;
                App.Current.Shutdown();
            }
        }
    }
}