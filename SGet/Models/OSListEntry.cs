using Newtonsoft.Json.Linq;
using SGet.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SGet.Models
{
    public class OSListEntry : INotifyPropertyChanged
    {
        private static NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        #region Fields and Properties_John

        //OS Wim Download Client
        public WebDownloadClient DownloadClient_OSWim { get; set; }

        //Boot Wim Download Client
        public WebDownloadClient DownloadClient_BootWim { get; set; }

        //OS's Model Names
        public List<string> ModelArray { get; set; }

        //OS's Model Names Friendly String
        public string Models {
            get {
                return "[" + string.Join("], [", this.ModelArray) + "]";
            }
        }

        //OS's Name
        public string Name { get; set; }

        //OS's Unique Identifier
        public string UniqueIdentifier { get; }

        public string ReleaseNotes { get; set; }

        public ObservableCollection<Public_Version_Table> PublicVersionTable { get; set; }

        // Percentage of downloaded data across both downloads (each contribution scaled by download size)
        public float Percent
        {
            get
            {
                return ((DownloadClient_OSWim.Percent * (float)DownloadClient_OSWim.FileSize) + (DownloadClient_BootWim.Percent * (float)DownloadClient_BootWim.FileSize)) / ((float)DownloadClient_OSWim.FileSize + (float)DownloadClient_OSWim.FileSize);
            }
        }

        public string PercentString
        {
            get
            {
                if (Percent < 0 || float.IsNaN(Percent))
                    return "0.0%";
                else if (Percent > 100)
                    return "100.0%";
                else
                    return String.Format(numberFormat, "{0:0.0}%", Percent);
            }
        }

        // Progress bar value
        public float Progress
        {
            get
            {
                return Percent;
            }
        }

        // Download speed
        public int downloadSpeed;
        public string DownloadSpeed
        {
            get
            {
                int combinedDownloadSpeed = 0;
                if (DownloadClient_OSWim.Status == DownloadStatus.Downloading && !DownloadClient_OSWim.HasError)
                {
                    combinedDownloadSpeed += DownloadClient_OSWim.downloadSpeed;
                }
                if (DownloadClient_BootWim.Status == DownloadStatus.Downloading && !DownloadClient_BootWim.HasError)
                {
                    combinedDownloadSpeed += DownloadClient_OSWim.downloadSpeed;
                }

                if (combinedDownloadSpeed > 0)
                {
                    return DownloadManager.FormatSpeedString(combinedDownloadSpeed);
                }
                return String.Empty;
            }
        }

        public ObservableCollection<string> ActionOptions { get; set; }

        private OSListRecordAction _selected_action;
        public string SelectedActionString
        {
            get
            {
                return OSListRecordEnumUtils.getActionDescriptionFromActionEnum(_selected_action);
            }
            set
            {
                _selected_action = OSListRecordEnumUtils.getActionEnumFromActionDescription(value);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedActionString"));
                //update status if the action requires it
                OSListRecordStatus? newstatus = OSListRecordEnumUtils.getNewOSListRecordStatusFromActionOption(_selected_action, status);
                if (newstatus != null && newstatus != status)
                {
                    Status = (OSListRecordStatus)newstatus;
                    RaiseStatusChanged();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusString"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ProgressBarVisibility"));
                        //PropertyChanged(this, new PropertyChangedEventArgs("ActionDropDownVisibility"));
                    }
                }
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return status == OSListRecordStatus.To_Be_Added ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ActionDropDownVisibility
        {
            get
            {
                // if (ActionOptions.Count != 2)
                //{
                return ActionOptions.Count == 0 || (ActionOptions.Count == 1 && ActionOptions[0].Length == 0) ? Visibility.Hidden : Visibility.Visible;
                //}
                //return Visibility.Visible;
            }
        }


        // Download status
        private OSListRecordStatus status;
        public OSListRecordStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                //set data for the drop down box in grid to bind to based on the Status
                string oldSelectedAction = SelectedActionString;
                ActionOptions = new ObservableCollection<string>(Array.ConvertAll(
                    OSListRecordEnumUtils.getActionOptionsFromOSListRecordStatus(status), x => OSListRecordEnumUtils.getActionDescriptionFromActionEnum(x)
                ));

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ActionDropDownVisibility"));
                }

                if (oldSelectedAction != null && !oldSelectedAction.Equals("") && ActionOptions.Contains(oldSelectedAction, StringComparer.CurrentCultureIgnoreCase))
                {
                    SelectedActionString = oldSelectedAction;
                }
                else
                {
                    SelectedActionString = OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordEnumUtils.getDefaultActionFromOSListRecordStatus(status));
                }
            }
        }

        public string StatusString
        {
            get
            {
                if (Status == OSListRecordStatus.Active)
                {
                    DownloadStatus overallStatus = DownloadStatus.Initialized;

                    //if both downloads are completed then return completed
                    if (DownloadClient_BootWim.Status == DownloadStatus.Completed && DownloadClient_OSWim.Status == DownloadStatus.Completed)
                    {
                        return DownloadStatus.Completed.ToString().Replace('_', ' ');
                    }

                    ////////////////////////////////////////

                    //if one is Paused then updated overallstatus to Paused then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Paused || DownloadClient_OSWim.Status == DownloadStatus.Paused)
                    {
                        overallStatus = DownloadStatus.Paused;
                    }
                    //if one is Deleted then updated overallstatus to Deleted then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Deleted || DownloadClient_OSWim.Status == DownloadStatus.Deleted)
                    {
                        overallStatus = DownloadStatus.Deleted;
                    }

                    //if one is queued then updated overallstatus to Queued then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Queued || DownloadClient_OSWim.Status == DownloadStatus.Queued)
                    {
                        overallStatus = DownloadStatus.Queued;
                    }

                    //if one is Waiting then updated overallstatus to Waiting then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Waiting || DownloadClient_OSWim.Status == DownloadStatus.Waiting)
                    {
                        overallStatus = DownloadStatus.Waiting;
                    }

                    //if one is Downloading then updated overallstatus to Downloading then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Downloading || DownloadClient_OSWim.Status == DownloadStatus.Downloading)
                    {
                        overallStatus = DownloadStatus.Downloading;
                    }

                    //if one is Error then updated overallstatus to Error then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Error || DownloadClient_OSWim.Status == DownloadStatus.Error)
                    {
                        overallStatus = DownloadStatus.Error;
                    }

                    //pausing or deleting should only be brief so they should overwride the main downloading and error status
                    //if one is Deleting then updated overallstatus to Deleting then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Deleting || DownloadClient_OSWim.Status == DownloadStatus.Deleting)
                    {
                        overallStatus = DownloadStatus.Deleting;
                    }
                    //if one is Pausing then updated overallstatus to Pausing then carry on checking
                    if (DownloadClient_BootWim.Status == DownloadStatus.Pausing || DownloadClient_OSWim.Status == DownloadStatus.Pausing)
                    {
                        overallStatus = DownloadStatus.Pausing;
                    }
                    return overallStatus.ToString().Replace('_', ' ');

                }
                return Status.ToString().Replace('_', ' ');
            }
        }

        // There was an error during download
        public bool HasError
        {
            get
            {
                return DownloadClient_BootWim.HasError || DownloadClient_OSWim.HasError;
            }
        }

        public bool AllowUserToAddToLibrary
        {
            get
            {
                if (SelectedActionString == OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Add_To_Library))
                {
                    return false;
                }
                if (SelectedActionString == OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Do_Not_Add))
                {
                    return true;
                }

                return status == OSListRecordStatus.Not_In_Library;
            }
        }

        public bool AllowUserToRemoveFromLibrary
        {
            get
            {
                if (SelectedActionString == OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Do_Not_Add))
                {
                    return false;
                }
                if (SelectedActionString == OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Add_To_Library))
                {
                    return true;
                }

                return status == OSListRecordStatus.To_Be_Added;
            }
        }

        #endregion
        #region Fields and Properties

        // Used for updating download speed on the DataGrid
        private int speedUpdateCount;

        // Average download speed
        public string AverageDownloadSpeed
        {
            get
            {
                return DownloadManager.FormatSpeedString((int)Math.Floor((double)(DownloadClient_OSWim.DownloadedSize + DownloadClient_OSWim.CachedSize + DownloadClient_BootWim.DownloadedSize + DownloadClient_BootWim.CachedSize) / TotalElapsedTime.TotalSeconds));
            }
        }

        // Time left to complete the download
        public string TimeLeft
        {
            get
            {
                return "NOT IMPLEMENTED";
                /*                if (recentAverageRate > 0 && this.Status == DownloadStatus.Downloading && !this.HasError)
                                {
                                    double secondsLeft = (FileSize - DownloadedSize + CachedSize) / recentAverageRate;

                                    TimeSpan span = TimeSpan.FromSeconds(secondsLeft);

                                    return DownloadManager.FormatTimeSpanString(span);
                                }
                                return String.Empty;*/
            }
        }

        // Elapsed time (doesn't include the time period when the download was paused)
        public TimeSpan ElapsedTime = new TimeSpan();

        // Time when the download was last started
        private DateTime lastStartTime;

        // Total elapsed time (includes the time period when the download was paused)
        public TimeSpan TotalElapsedTime
        {
            get
            {
                if (DownloadClient_OSWim.Status != DownloadStatus.Downloading && DownloadClient_BootWim.Status != DownloadStatus.Downloading)
                {
                    return ElapsedTime;
                }
                else
                {
                    return ElapsedTime.Add(DateTime.UtcNow - lastStartTime);
                }
            }
        }

        public string TotalElapsedTimeString
        {
            get
            {
                return DownloadManager.FormatTimeSpanString(TotalElapsedTime);
            }
        }

        // Time and size of downloaded data in the last calculaction of download speed
        private DateTime lastNotificationTime;
        private long lastNotificationDownloadedSize;


        // Date and time when the download was completed
        public DateTime CompletedOn { get; set; }
        public string CompletedOnString
        {
            get
            {
                if (DownloadClient_OSWim.CompletedOn != DateTime.MinValue && DownloadClient_BootWim.CompletedOn != DateTime.MinValue)
                {
                    string format = "dd.MM.yyyy. HH:mm:ss";
                    return (DownloadClient_OSWim.CompletedOn > DownloadClient_BootWim.CompletedOn ? DownloadClient_OSWim.CompletedOn : DownloadClient_BootWim.CompletedOn).ToString(format);
                }
                else
                    return String.Empty;
            }
        }

        // OS is selected in the DataGrid
        public bool IsSelected {
            get
            {
                return DownloadClient_BootWim.IsSelected && DownloadClient_OSWim.IsSelected;
            }
            set
            {
                DownloadClient_OSWim.IsSelected = value;
                DownloadClient_BootWim.IsSelected = value;
                if(value)
                {
                    RaisePropertyChanged(nameof(ReleaseNotes));
                }
                /*                if (PropertyChanged != null)
                                {
                                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                                }*/
            }
        }

        // Download is part of a batch
        public bool IsBatch
        {
            get
            {
                return DownloadClient_BootWim.IsBatch && DownloadClient_OSWim.IsBatch;
            }
            set
            {
                DownloadClient_OSWim.IsBatch = value;
                DownloadClient_BootWim.IsBatch = value;
            }
        }

        // Batch URL was checked
        public bool BatchUrlChecked
        {
            get
            {
                return DownloadClient_BootWim.BatchUrlChecked && DownloadClient_OSWim.BatchUrlChecked;
            }
            set
            {
                DownloadClient_OSWim.BatchUrlChecked = value;
                DownloadClient_BootWim.BatchUrlChecked = value;
            }
        }

        // Speed limit was changed
        public bool SpeedLimitChanged { get; set; }

        // Download buffer count per notification (DownloadProgressChanged event)
        public int BufferCountPerNotification { get; set; }

        #endregion

        #region Constructor and Events

        public OSListEntry(string uniqueIdentifier, List<string> models, string name,
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable, SGet.MainWindow mainWindow
        )
        {

            this.UniqueIdentifier = uniqueIdentifier;
            this.ModelArray = models;
            this.Name = name;
            this.ReleaseNotes = releaseNotes;

            //get existing or make new WebDownloadClient for OS Wim URL
            WebDownloadClient existingOSWimDownloadClient = DownloadManager.findDownloadInstanceByURLFromAllDownloadClientStore(osWimURL);
            if (existingOSWimDownloadClient == null)
            {
                this.DownloadClient_OSWim = new WebDownloadClient(osWimURL);
                this.DownloadClient_OSWim.FileSize = osWimFileSize;
                this.DownloadClient_OSWim.DownloadProgressChanged += this.DownloadClient_OSWim.DownloadProgressChangedHandler;
                this.DownloadClient_OSWim.DownloadCompleted += this.DownloadClient_OSWim.DownloadCompletedHandler;
                //this.DownloadClient_OSWim.StatusChanged += this.StatusChangedHandler;
                //this.DownloadClient_OSWim.DownloadCompleted += this.DownloadCompletedHandler;
                this.DownloadClient_OSWim.PropertyChanged += HandleDownloadClientPropertyChanged;

                if (osWimFileName == null || osWimFileName.Trim().Length == 0)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"The OS .Wim File Name for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DownloadClient_OSWim.HasError = true;
                }
                this.DownloadClient_OSWim.FileName = osWimFileName;
                DownloadManager.Instance.AllDownloadClientStore.Add(this.DownloadClient_OSWim);
            }
            else
            {
                this.DownloadClient_OSWim = existingOSWimDownloadClient;
            }



            //get existing or make new WebDownloadClient for Boot Wim URL
            WebDownloadClient existingBootWimDownloadClient = DownloadManager.findDownloadInstanceByURLFromAllDownloadClientStore(bootWimURL);
            if (existingBootWimDownloadClient == null)
            {
                this.DownloadClient_BootWim = new WebDownloadClient(bootWimURL);
                this.DownloadClient_BootWim.FileSize = bootWimFileSize;
                this.DownloadClient_BootWim.DownloadProgressChanged += this.DownloadClient_BootWim.DownloadProgressChangedHandler;
                this.DownloadClient_BootWim.DownloadCompleted += this.DownloadClient_BootWim.DownloadCompletedHandler;
                //this.DownloadClient_BootWim.StatusChanged += this.StatusChangedHandler;
                //this.DownloadClient_BootWim.DownloadCompleted += this.DownloadCompletedHandler;
                this.DownloadClient_BootWim.PropertyChanged += HandleDownloadClientPropertyChanged;

                if (bootWimFileName == null || bootWimFileName.Trim().Length == 0)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"The Boot .Wim File Name for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DownloadClient_BootWim.HasError = true;
                }
                this.DownloadClient_BootWim.FileName = bootWimFileName;
                DownloadManager.Instance.AllDownloadClientStore.Add(this.DownloadClient_BootWim);
            }
            else
            {
                this.DownloadClient_BootWim = existingBootWimDownloadClient;
            }

            if (publicVersionTable == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"The Public Version Table for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.PublicVersionTable = new ObservableCollection<Public_Version_Table>();
                this.DownloadClient_OSWim.HasError = true;
            }
            else
            {
                this.PublicVersionTable = new ObservableCollection<Public_Version_Table>(publicVersionTable);
            }

            this.Status = OSListRecordStatus.Initialized;

            if (mainWindow != null)
            {
                this.PropertyChanged += mainWindow.OSListPropertyChangedHandler;
                this.StatusChanged += mainWindow.OSListEntryStatusChanged;
            }
            this.StatusChanged += DownloadManager.OSListEntryStatusChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StatusChanged;
        /*
        public event EventHandler DownloadProgressChanged;

        public event EventHandler DownloadCompleted;
        */
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

        // Generate StatusChanged event
        protected virtual void RaiseStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        public void HandleDownloadClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WebDownloadClient downloadClient = (WebDownloadClient)sender;
            string propertyName = e.PropertyName;

            if (propertyName == "StatusString")
            {
                RaisePropertyChanged(propertyName);
            }
            else
            {
                RaisePropertyChanged(propertyName);
            }

            if (e.PropertyName == "Status")
            {
                RaisePropertyChanged("StatusString");
                RaisePropertyChanged("Status");
                //this.Dispatcher.Invoke(new PropertyChangedEventHandler(OSListUpdatePropertiesList), sender, e);
            }
        }

        // Start or continue download
        public void Start()
        {
            Status = OSListRecordStatus.Active;
            this.DownloadClient_BootWim.Start();
            this.DownloadClient_OSWim.Start();
        }

        public void Pause()
        {
            //Status = OSListRecordStatus.Active;
            this.DownloadClient_BootWim.Pause();
            this.DownloadClient_OSWim.Pause();
        }
        #endregion
    }

}