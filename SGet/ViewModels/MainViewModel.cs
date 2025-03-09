using Newtonsoft.Json;
using SGet.Commands;
using SGet.Models;
using SGet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SGet.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //Static Resources for Button Controls
        private ImageSource checkForUpdatesIcon_Connected;
        private ImageSource checkForUpdatesIcon_Connecting;
        private ImageSource checkForUpdatesIcon_Syncing;

        //Data Contexts of Child Views/Controls
        private object _osViewControlDataContext;
        private object _downloadsControlDataContext;

        private object _cenralPanelControlDataContext;
        public object CenralPanelControlDataContext
        {
            get
            {
                return _cenralPanelControlDataContext;
            }
            set
            {
                if (_cenralPanelControlDataContext != value )
                {
                    _cenralPanelControlDataContext = value;
                    RaisePropertyChanged(nameof(CenralPanelControlDataContext));
                }
            }
        }

        //State Properties
        private bool _apiCallIsInProgress;
        private APIContentsRootObject _osInfoApiResponseObject;

        public bool DownloadsPaused { get; set; }

        private bool _windowInDownloadsViewMode;

        public bool WindowIsInDownloadsViewMode { 
            get {
                return _windowInDownloadsViewMode;
            }
            set {
                if (_windowInDownloadsViewMode != value || _cenralPanelControlDataContext == null ) //only perform logic if it's changing or if its initializing
                {
                    _windowInDownloadsViewMode = value;
                    CenralPanelControlDataContext = _windowInDownloadsViewMode ? _downloadsControlDataContext : _osViewControlDataContext;
                    CommandManager.InvalidateRequerySuggested(); //this causes ALL commands to re-evaluate their CanExecute Method, but thats the done thing apparantly\
                }
            }
        }

        

        //Control Bindings
        private ImageSource _apiUpdateButtonImage;
        public ImageSource ApiUpdateButtonImage { 
            get {
                return _apiUpdateButtonImage;
            } 
            set { 
                if ( _apiUpdateButtonImage != value) {
                    _apiUpdateButtonImage = value;
                    RaisePropertyChanged(nameof(ApiUpdateButtonImage));
                }
            } 
        }

        private string _apiUpdateButtonText;
        public string ApiUpdateButtonText {
            get
            {
                return _apiUpdateButtonText;
            }
            set
            {
                if (_apiUpdateButtonText != value)
                {
                    _apiUpdateButtonText = value;
                    RaisePropertyChanged(nameof(ApiUpdateButtonText));
                }
            }
        }

        private bool _apiUpdateButtonisAvailable;
        public bool ApiUpdateButtonisAvailable
        {
            get
            {
                return _apiUpdateButtonisAvailable;
            }
            set
            {
                if (_apiUpdateButtonisAvailable != value)
                {
                    _apiUpdateButtonisAvailable = value;
                    RaisePropertyChanged(nameof(ApiUpdateButtonisAvailable));
                }
            }
        }


        //Event handlers
        public event PropertyChangedEventHandler PropertyChanged;

        //Command Properties
        public ICommand ShowSettingsWindowCommand { get; set; }
        public ICommand ShowAboutWindowCommand { get; set; }
        public ICommand ToggleToDownloadsViewModeCommand { get; set; }
        public ICommand CheckForUpdatesCommand { get; set; }

        public MainViewModel()
        {
            //Static Resources for Button Controls
            checkForUpdatesIcon_Connected = new BitmapImage(new Uri("pack://application:,,,/SGet;component/Resources/connected.png"));
            checkForUpdatesIcon_Connecting = new BitmapImage(new Uri("pack://application:,,,/SGet;component/Resources/connecting.png"));
            checkForUpdatesIcon_Syncing = new BitmapImage(new Uri("pack://application:,,,/SGet;component/Resources/apisync.png"));

            //Control Bindings
            HandleNetworkAvailabilityChanged(false); //this willset api button states to reflect 'no internet' for now, will get reassessed after first render

            //Command Properties
            ShowSettingsWindowCommand = new RelayCommand(ShowSettingsWindow, CanShowSettingsWindow);
            ShowAboutWindowCommand = new RelayCommand(ShowAboutWindow, CanShowAboutWindow);
            ToggleToDownloadsViewModeCommand = new RelayCommand(ToggleToDownloadsViewMode, CanToggleToDownloadsViewMode);
            CheckForUpdatesCommand = new RelayCommand(CheckForUpdates, CanCheckForUpdates);

            //State Properties - General
            DownloadsPaused = true;
            _osViewControlDataContext = new OSViewModel();
            _downloadsControlDataContext = null;
            WindowIsInDownloadsViewMode = false;
            //State Properties - Check for updates API
            _apiCallIsInProgress = false;
            _osInfoApiResponseObject = null;


            //setup event handler for internet connection status monitor
            NetworkChange.NetworkAvailabilityChanged += HandleNetworkAvailabilityChanged;
        }

        //Event Handlers

        // Generate PropertyChanged event to update the UI
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void HandleNetworkAvailabilityChanged(object obj, NetworkAvailabilityEventArgs eventArgs)
        {
            HandleNetworkAvailabilityChanged(eventArgs.IsAvailable);
        }
        public void HandleNetworkAvailabilityChanged(bool isAvailable)
        {
            if (isAvailable)
            {
                ApiUpdateButtonImage = checkForUpdatesIcon_Connected;
                ApiUpdateButtonText = "Check for Updates";
                ApiUpdateButtonisAvailable = true;
                if (_osInfoApiResponseObject == null)
                {
                    CheckForUpdates( null );
                }
            }
            else
            {
                ApiUpdateButtonImage = checkForUpdatesIcon_Connecting;
                ApiUpdateButtonText = "Awaiting Internet";
                ApiUpdateButtonisAvailable = false;
            }
        }

        internal void HandleOnFirstRender()
        {
            HandleNetworkAvailabilityChanged(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable());
        }

        //Button Commands
        private bool CanToggleToDownloadsViewMode(object param_FilesMode_OBJECT)
        {
            bool param_FilesMode = param_FilesMode_OBJECT == null ? false : (bool)param_FilesMode_OBJECT;
            return param_FilesMode != WindowIsInDownloadsViewMode;
        }

        private void ToggleToDownloadsViewMode(object param_FilesMode_OBJECT)
        {
            bool param_FilesMode = param_FilesMode_OBJECT == null ? false : (bool)param_FilesMode_OBJECT;
            this.WindowIsInDownloadsViewMode = param_FilesMode;
            
        }

        private bool CanShowSettingsWindow ( object param_StartOnRateLimitTab_OBJECT )
        {
            //bool param_doStartOnRateLimitTab = param_StartOnRateLimitTab_OBJECT == null ? false : (bool)param_StartOnRateLimitTab_OBJECT;
            return true;
        }

        private void ShowSettingsWindow( object param_StartOnRateLimitTab_OBJECT)
        {
            bool param_doStartOnRateLimitTab = param_StartOnRateLimitTab_OBJECT == null ? false : (bool)param_StartOnRateLimitTab_OBJECT;
            Preferences prferencesWindowView = new Preferences(param_doStartOnRateLimitTab);
            prferencesWindowView.ShowDialog();
        }

        private bool CanShowAboutWindow(object obj)
        {
            return true;
        }

        private void ShowAboutWindow(object obj)
        {
            About aboutWindowView = new About();
            aboutWindowView.ShowDialog();
        }

        public bool CanCheckForUpdates(object obj)
        {
            return ApiUpdateButtonisAvailable && !_apiCallIsInProgress;
        }

        public async void CheckForUpdates(object obj)
        {
            if (!_apiCallIsInProgress)
            {
                _apiCallIsInProgress = true;  //flag api as in use so as to disallow multiple simultaneous API calls

                //set button properties
                ApiUpdateButtonImage = checkForUpdatesIcon_Syncing;
                ApiUpdateButtonText = "Checking for Updates";
                ApiUpdateButtonisAvailable = true;

                //make API call using async await so as not to mash up the main UI thread
                _osInfoApiResponseObject = await APIHandler.getAPIResponseObjectAsync();

                string downloadsFolder = Settings.Default.DownloadLocation;
                foreach (Model model in _osInfoApiResponseObject.models)
                {
                    foreach (Redisguis os in model.redisguises)
                    {
                        OSListManager.AddOS(
                            os.redisguise_handle,
                            model.name,
                            os.redisguise_name,
                            os.aws_url_os_wim,
                            os.os_wim_file_name,
                            os.os_wim_file_size,
                            os.aws_url_boot_wim,
                            os.boot_wim_file_name,
                            (long)os.boot_wim_file_size,
                            os.release_notes,
                            os.public_version_table,
                            null,
                            downloadsFolder
                        );
                    }
                }

                //refresh button text now api sync has finished
                HandleNetworkAvailabilityChanged(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable());

                _apiCallIsInProgress = true;  //flag api 'can be used' now we're finished with it
            }
        }
    }
}
