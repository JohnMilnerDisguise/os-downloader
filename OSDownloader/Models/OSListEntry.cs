﻿using Newtonsoft.Json.Linq;
using OSDownloader.Properties;
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
using System.Windows.Input;
using System.Xml.Linq;

namespace OSDownloader.Models
{
    public class OSListEntry : INotifyPropertyChanged
    {
        private static NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        #region Fields and Properties_John

        //OS Wim Download Client
        public OSListEntryFile FileObject_OSWim { get; set; }

        //Boot Wim Download Client
        public OSListEntryFile FileObject_BootWim { get; set; }

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

        private string _releaseNotes;
        public string ReleaseNotes { get { return _releaseNotes; } set {
                if( _releaseNotes != value )
                {
                    _releaseNotes = value;
                    RaisePropertyChanged( nameof( ReleaseNotes ) );
                }
            } 
        }

        public ObservableCollection<Public_Version_Table> PublicVersionTable { get; set; }

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
                RaisePropertyChanged(nameof(SelectedActionString));
                //update status if the action requires it
                OSListRecordStatus? newstatus = OSListRecordEnumUtils.getNewOSListRecordStatusFromActionOption(_selected_action, status);
                if (newstatus != null && newstatus != status)
                {
                    Status = (OSListRecordStatus)newstatus;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return status == OSListRecordStatus.To_Be_Added ||
                       status == OSListRecordStatus.Active ||
                       status == OSListRecordStatus.Paused ? 
                       Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ActionDropDownVisibility
        {
            get
            {
                // if (ActionOptions.Count != 2)
                //{
                return ActionOptions == null || ActionOptions.Count == 0 || (ActionOptions.Count == 1 && ActionOptions[0].Length == 0) ? Visibility.Hidden : Visibility.Visible;
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
                if (status != value)
                {
                    status = value;
                    //set data for the drop down box in grid to bind to based on the Status
                    string oldSelectedAction = SelectedActionString;
                    ActionOptions = new ObservableCollection<string>(Array.ConvertAll(
                        OSListRecordEnumUtils.getActionOptionsFromOSListRecordStatus(status), x => OSListRecordEnumUtils.getActionDescriptionFromActionEnum(x)
                    ));

                    RaisePropertyChanged(nameof(ActionDropDownVisibility));
                    RaisePropertyChanged(nameof(ProgressBarVisibility));
                    RaisePropertyChanged(nameof(StatusString));
                    RaiseStatusChanged();

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
        }

        public string StatusString
        {
            get
            {
                if (Status == OSListRecordStatus.Active)
                {
                    DownloadStatus overallStatus = DownloadStatus.Initialized;

                    DownloadStatus? bootWimDownloadClientStatus = FileObject_BootWim.DownloadClient == null ? null : (DownloadStatus?)FileObject_BootWim.DownloadClient.Status;
                    DownloadStatus? osWimDownloadClientStatus = FileObject_OSWim.DownloadClient == null ? null : (DownloadStatus?)FileObject_OSWim.DownloadClient.Status;

                    //if both downloads are completed then return completed
                    if( ( bootWimDownloadClientStatus == null || bootWimDownloadClientStatus == DownloadStatus.Completed ) && 
                        ( osWimDownloadClientStatus == null || osWimDownloadClientStatus == DownloadStatus.Completed ) )
                    {
                        return DownloadStatus.Completed.ToString().Replace('_', ' ');
                    }

                    ////////////////////////////////////////

                    //if one is Paused then updated overallstatus to Paused then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Paused || osWimDownloadClientStatus == DownloadStatus.Paused)
                    {
                        overallStatus = DownloadStatus.Paused;
                    }
                    //if one is Deleted then updated overallstatus to Deleted then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Deleted || osWimDownloadClientStatus == DownloadStatus.Deleted)
                    {
                        overallStatus = DownloadStatus.Deleted;
                    }

                    //if one is queued then updated overallstatus to Queued then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Queued || osWimDownloadClientStatus == DownloadStatus.Queued)
                    {
                        overallStatus = DownloadStatus.Queued;
                    }

                    //if one is Waiting then updated overallstatus to Waiting then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Waiting || osWimDownloadClientStatus == DownloadStatus.Waiting)
                    {
                        overallStatus = DownloadStatus.Waiting;
                    }

                    //if one is Downloading then updated overallstatus to Downloading then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Downloading || osWimDownloadClientStatus == DownloadStatus.Downloading)
                    {
                        overallStatus = DownloadStatus.Downloading;
                    }

                    //if one is Error then updated overallstatus to Error then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Error || osWimDownloadClientStatus == DownloadStatus.Error)
                    {
                        overallStatus = DownloadStatus.Error;
                    }

                    //pausing or deleting should only be brief so they should overwride the main downloading and error status
                    //if one is Deleting then updated overallstatus to Deleting then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Deleting || osWimDownloadClientStatus == DownloadStatus.Deleting)
                    {
                        overallStatus = DownloadStatus.Deleting;
                    }
                    //if one is Pausing then updated overallstatus to Pausing then carry on checking
                    if (bootWimDownloadClientStatus == DownloadStatus.Pausing || osWimDownloadClientStatus == DownloadStatus.Pausing)
                    {
                        overallStatus = DownloadStatus.Pausing;
                    }
                    return overallStatus.ToString().Replace('_', ' ');

                }
                return Status.ToString().Replace('_', ' ');
            }
        }

        // Percentage of downloaded data across both downloads (each contribution scaled by download size)
        public float Percent
        {
            get
            {
                float bootWimDownloadSize = (float)( FileObject_BootWim.DownloadClient == null ? 0 : FileObject_BootWim.DownloadClient.FileSize );
                float osWimDownloadSize = (float)( FileObject_OSWim.DownloadClient == null ? 0 : FileObject_OSWim.DownloadClient.FileSize );
                float bootWimDownloadPercent = bootWimDownloadSize == 0f ? 0f : FileObject_BootWim.DownloadClient.Percent;
                float osWimDownloadPercent = osWimDownloadSize == 0f ? 0f : FileObject_OSWim.DownloadClient.Percent;
                if ( bootWimDownloadSize + osWimDownloadSize == 0f ) {
                    //avoid divide by zero exception
                    return 0f;
                }
                return ( (osWimDownloadPercent * osWimDownloadSize) + (bootWimDownloadPercent * bootWimDownloadSize)) / (osWimDownloadSize + bootWimDownloadSize);
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
                if (FileObject_BootWim.DownloadClient != null && Status == DownloadStatus.Downloading && !DownloadClient_BootWim.HasError)
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

        #region Event Handler Properties
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StatusChanged;
        /*
        public event EventHandler DownloadProgressChanged;

        public event EventHandler DownloadCompleted;
        */
        #endregion

        #region Constructor and Events

        public OSListEntry(string uniqueIdentifier, List<string> models, string name,
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable, OSDownloader.MainWindow mainWindow
        )
        {

            this.UniqueIdentifier = uniqueIdentifier;
            this.ModelArray = models;
            this.Name = name;
            this.ReleaseNotes = releaseNotes;

            //get existing or make new WebDownloadClient for OS Wim URL
            FileObject_OSWim = new OSListEntryFile(this, "OS .Wim", osWimURL, osWimFileSize, osWimFileName);

            //get existing or make new WebDownloadClient for Boot Wim URL
            FileObject_BootWim = new OSListEntryFile(this, "Boot .Wim", bootWimURL, bootWimFileSize, bootWimFileName);

            if (publicVersionTable == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"The Public Version Table for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.PublicVersionTable = new ObservableCollection<Public_Version_Table>();
                this.FileObject_OSWim.DownloadClient.HasError = true;
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

            RaisePropertyChanged(propertyName);

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

        public void HandleDownloadClientStatusChanged(object sender, EventArgs e)
        {
            WebDownloadClient downloadClient = (WebDownloadClient)sender;
            RaisePropertyChanged(nameof(StatusString));
        }



        // Start or continue download
        public void Start()
        {
            Status = OSListRecordStatus.Active;
            if( this.FileObject_OSWim.DownloadClient != null )
            {
                this.FileObject_OSWim.DownloadClient.Start();
            }
            if (this.FileObject_BootWim.DownloadClient != null)
            {
                this.FileObject_BootWim.DownloadClient.Start();
            }
        }

        public void Pause()
        {
            //Status = OSListRecordStatus.Active;
            Status = OSListRecordStatus.Paused;
            if (this.FileObject_OSWim.DownloadClient != null)
            {
                this.FileObject_OSWim.DownloadClient.Pause();
            }
            if (this.FileObject_BootWim.DownloadClient != null)
            {
                this.FileObject_BootWim.DownloadClient.Pause();
            }
        }
        #endregion
    }

}