using SGet.Properties;
using SGet.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using SGet.ViewModels;

namespace SGet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private string[] args;

        private bool trayExit;

        private bool onRenderEventHasFiredOnceAlready;

        //private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            //initialise instance Variables & Properties
            args = Environment.GetCommandLineArgs();
            trayExit = false;
            onRenderEventHasFiredOnceAlready = false;

            //Initialise the view model to be used as the datasource for this window
            // mainViewModel = new MainViewModel();
            //Set DataContext of main Window
            //this.DataContext = mainViewModel;

            //Set show in Taskbar Behaviour
            if (!Settings.Default.ShowWindowOnStartup && args.Length != 2)
            {
                this.ShowInTaskbar = false;
                this.Visibility = Visibility.Hidden;
            }


        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            //CONTENT REMOVED
        }

        public void StatusChangedHandler(object sender, EventArgs e)
        {
            //CONTENT REMOVED
        }

        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            //CONTENT REMOVED
        }

        public void OSListPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            //CONTENT REMOVED
        }

        public void OSListEntryStatusChanged(object sender, EventArgs e)
        {
            //CONTENT REMOVED
        }

        #region Main Window Event Handlers
        private async void mainWindow_ContentRendered(object sender, EventArgs e)
        {
            // In case the application was started from a web browser and receives command-line arguments
            if (args.Length == 2)
            {
                if (args[1].StartsWith("http"))
                {
                    System.Windows.Clipboard.SetText(args[1]);

                    NewDownload newDownloadDialog = new NewDownload(this);
                    newDownloadDialog.ShowDialog();
                }
            }
            // JOHNS CODE //
            if (!onRenderEventHasFiredOnceAlready)
            {
                onRenderEventHasFiredOnceAlready = true;
                MainViewModel viewModel = (MainViewModel)this.DataContext;
                viewModel?.HandleOnFirstRender();
            }
            // END OF JOHNS CODE //
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //CONTENT REMOVED
        }

        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized && Settings.Default.MinimizeToTray)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Settings.Default.CloseToTray && !trayExit)
            {
                this.Hide();
                e.Cancel = true;
                return;
            }

            if (Settings.Default.ConfirmExit)
            {
                string message = "Are you sure you want to exit the application?";
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "SGet", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    trayExit = false;
                    return;
                }
            }

            //SaveDownloadsToXml();
        }

        #endregion
    }

}
