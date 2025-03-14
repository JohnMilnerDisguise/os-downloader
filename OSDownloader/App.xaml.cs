﻿using Microsoft.Shell;
using OSDownloader.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace OSDownloader
{
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "OSDownloader";

        #region Methods

        public App()
        {
            //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.All;
        }

        // Catch exceptions which occur outside try-catch blocks
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Xceed.Wpf.Toolkit.MessageBox.Show(e.Exception.Message + "\n\n" + e.Exception.InnerException.ToString(), "Error");
            e.Handled = true;
            Application.Current.Shutdown();
        }

        // Check if the Add New Download window is already open
        public static bool IsWindowAlreadyOpen(Type WindowType)
        {
            foreach (Window OpenWindow in Application.Current.Windows)
            {
                if (OpenWindow.GetType() == WindowType)
                    return true;
            }
            return false;
        }

        // This method is executed if you start the OSDownloader process from an external application, e.g. a web browser which sends the URL of the file to download
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Check if the command-line arguments were sent and if the NewDownload window is already open
            if ((args.Count == 2) && args[1].ToString().StartsWith("http") && !IsWindowAlreadyOpen(typeof(NewDownload)))
            {
                // The first argument args[0] contains the path to the OSDownloader process
                // The second argument args[1] contains the URL of the file to download, which is set as text in the Clipboard
                Clipboard.SetText(args[1].ToString());

                // Open the Add New Download window and ensure it's on the top
                NewDownload newDownloadDialog = new NewDownload((MainWindow)(Application.Current.MainWindow));
                newDownloadDialog.Topmost = true;
                newDownloadDialog.ShowDialog();
            }

            return true;
        }

        #endregion

        [STAThread]
        public static void Main()
        {
            // Ensure there can be only one instance of the application
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                //Reload Settings if and only if the settings file has changed
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                }

                var application = new App();

                application.InitializeComponent();
                application.Run();

                SingleInstance<App>.Cleanup();
            }
        }
    }
}
