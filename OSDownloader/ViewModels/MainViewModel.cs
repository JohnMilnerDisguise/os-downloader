﻿using Newtonsoft.Json;
using OSDownloader.Commands;
using OSDownloader.Models;
using OSDownloader.Models.JSON;
using OSDownloader.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace OSDownloader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region ALL Properties

        //ViewModel Event handlers
        public event PropertyChangedEventHandler PropertyChanged;

        //Static Resources for Button Controls
        private ImageSource checkForUpdatesIcon_Connected;
        private ImageSource checkForUpdatesIcon_Connecting;
        private ImageSource checkForUpdatesIcon_Syncing;

        //Data Contexts of Child Views/Controls
        private ICentralPanelBaseViewModel _pleaseWaitViewControlDataContext;
        private ICentralPanelBaseViewModel _osViewControlDataContext;
        private ICentralPanelBaseViewModel _downloadsControlDataContext;

        private ICentralPanelBaseViewModel _cenralPanelControlDataContext;
        public ICentralPanelBaseViewModel CenralPanelControlDataContext
        {
            get
            {
                return _cenralPanelControlDataContext;
            }
            set
            {
                if (_cenralPanelControlDataContext != value)
                {
                    _cenralPanelControlDataContext = value;
                    RaisePropertyChanged(nameof(CenralPanelControlDataContext));
                }
            }
        }

        //State Properties
        private bool _apiCallIsInProgress;
        public bool ApiCallIsInProgress
        {
            get
            {
                return _apiCallIsInProgress;
            }
            set
            {
                if (_apiCallIsInProgress != value)
                {
                    _apiCallIsInProgress = value;
                    ((RelayCommand)CheckForUpdatesCommand).RaiseCanExecuteChanged(); //this causes UI to refresh IsEnabled
                }
                CenralPanelControlDataContext = _apiCallIsInProgress ? _pleaseWaitViewControlDataContext : (_windowInDownloadsViewMode ? _downloadsControlDataContext : _osViewControlDataContext);
            }
        }

        private APIContentsRootObject _osInfoApiResponseObject;

        private bool _downloadsPaused;
        public bool DownloadsPaused {
            get { return _downloadsPaused; }
            set {
                if (value != _downloadsPaused)
                {
                    _downloadsPaused = value;
                    ((RelayCommand)PauseDownloadsCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)UnPauseDownloadsCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _windowInDownloadsViewMode;

        public bool WindowIsInDownloadsViewMode {
            get {
                return _windowInDownloadsViewMode;
            }
            set {
                if (_windowInDownloadsViewMode != value || _cenralPanelControlDataContext == null) //only perform logic if it's changing or if its initializing
                {
                    _windowInDownloadsViewMode = value;
                    ((RelayCommand)ToggleToDownloadsViewModeCommand).RaiseCanExecuteChanged(); //this causes UI to refresh IsEnabled
                }
                CenralPanelControlDataContext = _apiCallIsInProgress ? _pleaseWaitViewControlDataContext : (_windowInDownloadsViewMode ? _downloadsControlDataContext : _osViewControlDataContext);
            }
        }



        //Control Bindings
        private ImageSource _apiUpdateButtonImage;
        public ImageSource ApiUpdateButtonImage {
            get {
                return _apiUpdateButtonImage;
            }
            set {
                if (_apiUpdateButtonImage != value) {
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
                    ((RelayCommand)CheckForUpdatesCommand).RaiseCanExecuteChanged(); //this causes UI to refresh IsEnabled
                }
            }
        }

        //Disk Space Control Bindings
        public string PathToEXEIfRunningFromUSB;

        private string _tempDiskNameLabelContent;
        public string TempDiskNameLabelContent {
            get => _tempDiskNameLabelContent;
            set
            {
                if (_tempDiskNameLabelContent != value)
                {
                    _tempDiskNameLabelContent = value;
                    RaisePropertyChanged(nameof(TempDiskNameLabelContent));
                }
            }
        }

        private double _tempDiskTotalSpace;
        public double TempDiskTotalSpace
        {
            get => _tempDiskTotalSpace;
            set
            {
                if (_tempDiskTotalSpace != value)
                {
                    _tempDiskTotalSpace = value;
                    RaisePropertyChanged(nameof(TempDiskTotalSpace));
                }
            }
        }


        private double _tempDiskUsedSpace;
        public double TempDiskUsedSpace
        {
            get => _tempDiskUsedSpace;
            set
            {
                if (_tempDiskUsedSpace != value)
                {
                    _tempDiskUsedSpace = value;
                    RaisePropertyChanged(nameof(TempDiskUsedSpace));
                }
            }
        }

        private double _tempDiskRequiredSpace;
        public double TempDiskRequiredSpace
        {
            get => _tempDiskRequiredSpace;
            set
            {
                if (_tempDiskRequiredSpace != value)
                {
                    _tempDiskRequiredSpace = value;
                    RaisePropertyChanged(nameof(TempDiskRequiredSpace));
                }
            }
        }

        private double _usbDiskTotalSpace;
        public double USBDiskTotalSpace
        {
            get => _usbDiskTotalSpace;
            set
            {
                if (_usbDiskTotalSpace != value)
                {
                    _usbDiskTotalSpace = value;
                    RaisePropertyChanged(nameof(USBDiskTotalSpace));
                }
            }
        }

        private double _usbDiskUsedSpace;
        public double USBDiskUsedSpace
        {
            get => _usbDiskUsedSpace;
            set
            {
                if (_usbDiskUsedSpace != value)
                {
                    _usbDiskUsedSpace = value;
                    RaisePropertyChanged(nameof(USBDiskUsedSpace));
                }
            }
        }

        private double _usbDiskRequiredSpace;
        public double USBDiskRequiredSpace
        {
            get => _usbDiskRequiredSpace;
            set
            {
                if (_usbDiskRequiredSpace != value)
                {
                    _usbDiskRequiredSpace = value;
                    RaisePropertyChanged(nameof(USBDiskRequiredSpace));
                }
            }
        }

        //Status Bar Bindings
        public string StatusBarActiveDownloadsText { get { 
                return $"{DownloadManager.Instance.ActiveDownloads} Active Downloads"; 
        } }

        public string StatusBarCompletedDownloadsText { get {
                return $"{DownloadManager.Instance.CompletedDownloads} Completed Downloads";
        } }

        //Event Handlers

        //Command Properties
        public ICommand ShowSettingsWindowCommand { get; set; }
        public ICommand ShowAboutWindowCommand { get; set; }
        public ICommand ToggleToDownloadsViewModeCommand { get; set; }
        public ICommand CheckForUpdatesCommand { get; set; }
        public ICommand PauseDownloadsCommand { get; set; }
        public ICommand UnPauseDownloadsCommand { get; set; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            //Static Resources for Button Controls
            checkForUpdatesIcon_Connected = new BitmapImage(new Uri("pack://application:,,,/OSDownloader;component/Resources/connected.png"));
            checkForUpdatesIcon_Connecting = new BitmapImage(new Uri("pack://application:,,,/OSDownloader;component/Resources/connecting.png"));
            checkForUpdatesIcon_Syncing = new BitmapImage(new Uri("pack://application:,,,/OSDownloader;component/Resources/apisync.png"));

            //Control Bindings
            HandleNetworkAvailabilityChanged(false); //this willset api button states to reflect 'no internet' for now, will get reassessed after first render

            //Command Properties
            ShowSettingsWindowCommand = new RelayCommand(ShowSettingsWindow, CanShowSettingsWindow);
            ShowAboutWindowCommand = new RelayCommand(ShowAboutWindow, CanShowAboutWindow);
            ToggleToDownloadsViewModeCommand = new RelayCommand(ToggleToDownloadsViewMode, CanToggleToDownloadsViewMode);
            CheckForUpdatesCommand = new RelayCommand(CheckForUpdates, CanCheckForUpdates);
            PauseDownloadsCommand = new RelayCommand(PauseDownloads, CanPauseDownloads);
            UnPauseDownloadsCommand = new RelayCommand(UnPauseDownloads, CanUnPauseDownloads);

            //State Properties - General
            DownloadsPaused = true;
            WindowIsInDownloadsViewMode = false;

            //State Properties Child View Models / Data Contexts
            _pleaseWaitViewControlDataContext = (ICentralPanelBaseViewModel)(new PleaseWaitViewModel());
            _osViewControlDataContext = (ICentralPanelBaseViewModel)(new OSViewModel());
            _downloadsControlDataContext = (ICentralPanelBaseViewModel)(new DownloadsViewModel());

            //State Properties - Check for updates API
            ApiCallIsInProgress = false;
            _osInfoApiResponseObject = null;

            //Load an Array of files currently pre-existing in the customers OS Library (possibly remote and on another machine)
            OSListManager.Instance.LoadExistingDeploymentShareContentsFromJSON();
            //load an array of OS State objects from the last time the GUI Was closed so we can re-open the GUI in the same state
            OSListManager.Instance.LoadFullStateFromJSON();

            //setup event handler for internet connection status monitor
            NetworkChange.NetworkAvailabilityChanged += HandleNetworkAvailabilityChanged;

            //hook into Download Manager's StatusChanged Event to call HandleOneDownloadsStatusChanged whever a download's status changes
            DownloadManager.Instance.DownloadEntryStatusChangedHandler += HandleOneDownloadsStatusChanged;

            //set intitial data context for central panel
            CenralPanelControlDataContext = _pleaseWaitViewControlDataContext;

            //Set properties for Temp Drive Space Control
            string downloadsFolder = getDownloadsFolderPathAndPromptUserIfItsInvalid();
            RescanTempDriveAndUpdateProperties(downloadsFolder);
            TempDiskRequiredSpace = 0.0;

            //Set properties for USB Drive Space Control
            PathToEXEIfRunningFromUSB = DriveSpaceInfo.GetEXEPathIfRunningFromUSBDrive();
            RescanUSBDriveAndUpdateProperties(PathToEXEIfRunningFromUSB);
            USBDiskRequiredSpace = 0.0;
        }

        #endregion

        #region Event Handlers

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

        internal void HandleWindowClosing()
        {
            OSListManager.Instance.SaveFullStateToJSON();
        }

        

        public void HandleOneDownloadsStatusChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged( nameof(StatusBarActiveDownloadsText) );
            RaisePropertyChanged( nameof(StatusBarCompletedDownloadsText) );
        }

        #endregion

        #region Button Commands
        private bool CanUnPauseDownloads(object obj)
        {
            return DownloadsPaused;
        }

        private void UnPauseDownloads(object obj)
        {
            DownloadsPaused = false;
            foreach (OSListEntry osDownload in OSListManager.Instance.getPausedOSRecords())
            {
                if (osDownload.Status == OSListRecordStatus.Paused || osDownload.Status == OSListRecordStatus.To_Be_Added || osDownload.HasError)
                {
                    osDownload.Start();
                }
            }
        }

        private bool CanPauseDownloads(object obj)
        {
            return !DownloadsPaused;
        }

        private void PauseDownloads(object obj)
        {
            DownloadsPaused = true;
            OSListManager.Instance.SetAllActiveOSRecordsToPaused();
        }

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

        private bool CanShowSettingsWindow ( object param_StartOnNamedTab_OBJECT )
        {
            //string param_StartOnNamedTab = (string)param_StartOnNamedTab_OBJECT;
            return true;
        }

        private void ShowSettingsWindow( object param_StartOnNamedTab_OBJECT )
        {
            string param_StartOnNamedTab = (string)param_StartOnNamedTab_OBJECT;
            Preferences prferencesWindowView = new Preferences(param_StartOnNamedTab);
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
            System.Diagnostics.Debug.WriteLine("Evaluating CanCheckForUpdates");
            return ApiUpdateButtonisAvailable && !_apiCallIsInProgress;
        }

        public async void CheckForUpdates(object obj)
        {
            if (!_apiCallIsInProgress)
            {
                ApiCallIsInProgress = true;  //flag api as in use so as to disallow multiple simultaneous API calls

                //set button properties
                ApiUpdateButtonImage = checkForUpdatesIcon_Syncing;
                ApiUpdateButtonText = "Checking for Updates";
                ApiUpdateButtonisAvailable = false;

                //Get Downloads Folder location and exit early if it's invalid and user cancelled the setup
                string downloadsFolder = getDownloadsFolderPathAndPromptUserIfItsInvalid();
                if( downloadsFolder == null )
                {
                    HandleNetworkAvailabilityChanged(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable());
                    ApiCallIsInProgress = false;
                    return;
                }

                //make API call using async await so as not to mash up the main UI thread
                _osInfoApiResponseObject = await APIHandler.getAPIResponseObjectAsync();

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
                            downloadsFolder
                        );
                    }
                }

                //refresh button text now api sync has finished
                HandleNetworkAvailabilityChanged(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable());

                ApiCallIsInProgress = false;  //flag api 'can be used' now we're finished with it
            }
        }

        #endregion

        #region Convenience Methods
        public void RescanTempDriveAndUpdateProperties( string tempFolderPath )
        {
            DriveSpaceInfo tempDriveSpaceInfo = null;
            if (tempFolderPath != null)
            {
                tempDriveSpaceInfo = DriveSpaceInfo.GetDetailedDriveInfo(tempFolderPath);
            }
            if(tempDriveSpaceInfo == null)
            {
                TempDiskNameLabelContent = "Unknown Drive";
                TempDiskTotalSpace = 0.1;
                TempDiskUsedSpace = 0.1;
            }
            else
            {
                TempDiskNameLabelContent = tempDriveSpaceInfo.DriveLetter + " Drive";
                TempDiskTotalSpace = tempDriveSpaceInfo.TotalSizeGB * 1024.0 * 1024.0 * 1024.0;
                TempDiskUsedSpace = tempDriveSpaceInfo.UsedSpaceGB * 1024.0 * 1024.0 * 1024.0;
            }
        }

        public void RescanUSBDriveAndUpdateProperties(string usbExePath)
        {
            DriveSpaceInfo usbDriveSpaceInfo = null;
            if (usbExePath != null)
            {
                usbDriveSpaceInfo = DriveSpaceInfo.GetDetailedDriveInfo(usbExePath);
            }
            if (usbDriveSpaceInfo == null)
            {
                USBDiskTotalSpace = 0.0;
                USBDiskUsedSpace = 0.0;
            }
            else
            {
                USBDiskTotalSpace = usbDriveSpaceInfo.TotalSizeGB * 1024.0 * 1024.0 * 1024.0;
                USBDiskUsedSpace = usbDriveSpaceInfo.UsedSpaceGB * 1024.0 * 1024.0 * 1024.0;
            }
        }

        public string getDownloadsFolderPathAndPromptUserIfItsInvalid()
        {
            //Make Sure Downloads Folder Exists
            string downloadsFolder = Settings.Default.DownloadLocation;
            while (downloadsFolder == null || downloadsFolder.Trim().Length == 0 || !Directory.Exists(downloadsFolder))
            {
                string errorReason = null;
                if (downloadsFolder == null || downloadsFolder.Trim().Length == 0)
                {
                    string message = $"No Temporary Downloads Folder is set\n\nTo use OS Downloader you need to set up a Temporary Folder on this computer where your unfinished downloads will be stored.\n\nPlease Choose a Temp Downloads Location in your Settings.";
                    MessageBoxResult buttonPressed = Xceed.Wpf.Toolkit.MessageBox.Show(message, "No Temporary Downloads Folder Set", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (buttonPressed == MessageBoxResult.OK)
                    {
                        ShowSettingsWindow("tiLocation");
                        downloadsFolder = Settings.Default.DownloadLocation; //refresh the downloadsFolder with the new settings value
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                }
                string parentFolderPath = Path.GetDirectoryName(downloadsFolder.TrimEnd(new char[] { '\\' })); //this gets parent dir even not paths that dont exist

                if (!Directory.Exists(parentFolderPath))
                {
                    errorReason = $"the folder's parent [{parentFolderPath}] doesnt exist either";
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(downloadsFolder);
                    }
                    catch (UnauthorizedAccessException accessDeniedExcptionInstance)
                    {
                        errorReason = "permission was denied by Windows";
                    }
                }

                if (errorReason != null)
                {
                    string message = $"Could not create your temporary downloads folder:\n[{downloadsFolder}]\n\nThe Folder does not exist on your computer, and it couldnt be created automatically because {errorReason}.\n\nWould you like to edit this location in your Settings?";
                    MessageBoxResult buttonPressed = Xceed.Wpf.Toolkit.MessageBox.Show(message, "Invalid Temp Folder Location", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    if (buttonPressed == MessageBoxResult.OK)
                    {
                        ShowSettingsWindow("tiLocation");
                        downloadsFolder = Settings.Default.DownloadLocation; //refresh the downloadsFolder with the new settings value
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return downloadsFolder;
        }

        #endregion
    }
}
