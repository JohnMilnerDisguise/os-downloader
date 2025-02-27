using Newtonsoft.Json.Linq;
using SGet.Properties;
using System;
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
using System.Xml.Linq;

namespace SGet
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
                return "[" + string.Join( "], [", this.ModelArray ) + "]";
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
                return ( ( DownloadClient_OSWim.Percent * (float)DownloadClient_OSWim.FileSize ) + (DownloadClient_BootWim.Percent * (float)DownloadClient_BootWim.FileSize)) / ( (float)DownloadClient_OSWim.FileSize + (float)DownloadClient_OSWim.FileSize );
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

                if ( combinedDownloadSpeed > 0 )
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
                //update status if the action requires it
                OSListRecordStatus? newstatus = OSListRecordEnumUtils.getNewOSListRecordStatusFromActionOption(_selected_action, status);
                if (newstatus != null && newstatus != status)
                {
                    Status = (OSListRecordStatus)newstatus;
                    RaiseStatusChanged();
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusString"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ProgressBarVisibility"));
                }
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return status == OSListRecordStatus.Not_In_Library ? Visibility.Hidden : Visibility.Visible;
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

        // Status text in the DataGrid
        public string StatusText;
        public string StatusString
        {
            get
            {
                if (DownloadClient_BootWim.HasError)
                    return DownloadClient_BootWim.StatusText;
                else if (DownloadClient_OSWim.HasError)
                    return DownloadClient_OSWim.StatusText;
                else
                    return Status.ToString().Replace('_', ' ');
            }
            //set
            //{
            //    StatusText = value;
            //RaiseStatusChanged();
            //}
        }

        // There was an error during download
        public bool HasError
        {
            get
            {
                return DownloadClient_BootWim.HasError || DownloadClient_OSWim.HasError;
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

        #region Constructor and Events

        public OSListEntry(string uniqueIdentifier, List<string> models, string name,
            string osWimURL, string osWimFileName,
            string bootWimURL, string bootWimFileName,
            string releaseNotes, Public_Version_Table[] publicVersionTable, SGet.MainWindow mainWindow
        )
        {

            this.UniqueIdentifier = uniqueIdentifier;
            this.ModelArray = models;
            this.Name = name;
            this.ReleaseNotes = releaseNotes;
            this.DownloadClient_OSWim = new WebDownloadClient(osWimURL);
            
            if (osWimFileName == null || osWimFileName.Trim().Length == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"The OS .Wim File Name for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DownloadClient_OSWim.HasError = true;
            }
            this.DownloadClient_OSWim.FileName = osWimFileName;
          
            this.DownloadClient_BootWim = new WebDownloadClient(bootWimURL);
            if (bootWimFileName == null || bootWimFileName.Trim().Length == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"The Boot .Wim File Name for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DownloadClient_BootWim.HasError = true;
            }
            this.DownloadClient_BootWim.FileName = bootWimFileName;

            if (publicVersionTable == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"The Public Version Table for the {uniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.PublicVersionTable = new ObservableCollection<Public_Version_Table>();
            }
            else
            {
                this.PublicVersionTable = new ObservableCollection<Public_Version_Table>(publicVersionTable);
            }

            this.StatusText = String.Empty;
            this.Status = OSListRecordStatus.Initialized;

            this.PropertyChanged += mainWindow.OSListPropertyChangedHandler;
            this.StatusChanged += mainWindow.OSListEntryStatusChanged;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
    
        public event EventHandler StatusChanged;
        /*
        public event EventHandler DownloadProgressChanged;

        public event EventHandler DownloadCompleted;

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
        */
        // Generate StatusChanged event
        protected virtual void RaiseStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }
        /*

        // Generate DownloadProgressChanged event
        protected virtual void RaiseDownloadProgressChanged()
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, EventArgs.Empty);
            }
        }

        // Generate DownloadCompleted event
        protected virtual void RaiseDownloadCompleted()
        {
            if (DownloadCompleted != null)
            {
                DownloadCompleted(this, EventArgs.Empty);
            }
        }

        // DownloadProgressChanged event handler
        public void DownloadProgressChangedHandler(object sender, EventArgs e)
        {
            // Update the UI every second
            if (DateTime.UtcNow > this.LastUpdateTime.AddSeconds(1))
            {
                CalculateDownloadSpeed();
                CalculateAverageRate();
                UpdateDownloadDisplay();
                this.LastUpdateTime = DateTime.UtcNow;
            }
        }

        // DownloadCompleted event handler
        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            if (!this.HasError)
            {
                // If the file already exists, delete it
                if (File.Exists(this.DownloadPath))
                {
                    File.Delete(this.DownloadPath);
                }

                // Convert the temporary (.tmp) file to the actual (requested) file
                if (File.Exists(this.TempDownloadPath))
                {
                    File.Move(this.TempDownloadPath, this.DownloadPath);
                }

                this.Status = DownloadStatus.Completed;
                UpdateDownloadDisplay();

                if (this.OpenFileOnCompletion && File.Exists(this.DownloadPath))
                {
                    Process.Start(@DownloadPath);
                }
            }
            else
            {
                this.Status = DownloadStatus.Error;
                UpdateDownloadDisplay();
            }
        }

        #endregion

        #region Methods





        // Calculate download speed
        private void CalculateDownloadSpeed()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan interval = now - lastNotificationTime;
            double timeDiff = interval.TotalSeconds;
            double sizeDiff = (double)(DownloadedSize + CachedSize - lastNotificationDownloadedSize);

            downloadSpeed = (int)Math.Floor(sizeDiff / timeDiff);

            downloadRates.Add(downloadSpeed);

            lastNotificationDownloadedSize = DownloadedSize + CachedSize;
            lastNotificationTime = now;
        }

        // Calculate average download speed in the last 10 seconds
        private void CalculateAverageRate()
        {
            if (downloadRates.Count > 0)
            {
                if (downloadRates.Count > 10)
                    downloadRates.RemoveAt(0);

                int rateSum = 0;
                recentAverageRate = 0;
                foreach (int rate in downloadRates)
                {
                    rateSum += rate;
                }

                recentAverageRate = rateSum / downloadRates.Count;
            }
        }

        // Update download display (on downloadsGrid and propertiesGrid controls)
        private void UpdateDownloadDisplay()
        {
            RaisePropertyChanged("DownloadedSizeString");
            RaisePropertyChanged("PercentString");
            RaisePropertyChanged("Progress");

            // New download speed update every 4 seconds
            TimeSpan startInterval = DateTime.UtcNow - lastStartTime;
            if (speedUpdateCount == 0 || startInterval.TotalSeconds < 4 || this.HasError || this.Status == DownloadStatus.Paused
                || this.Status == DownloadStatus.Queued || this.Status == DownloadStatus.Completed)
            {
                RaisePropertyChanged("DownloadSpeed");
            }
            speedUpdateCount++;
            if (speedUpdateCount == 4)
                speedUpdateCount = 0;

            RaisePropertyChanged("TimeLeft");
            RaisePropertyChanged("StatusString");
            RaisePropertyChanged("CompletedOnString");

            if (this.IsSelected)
            {
                RaisePropertyChanged("AverageSpeedAndTotalTime");
            }
        }

        // Reset download properties to default values
        private void ResetProperties()
        {
            HasError = false;
            TempFileCreated = false;
            DownloadedSize = 0;
            CachedSize = 0;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            downloadRates.Clear();
            ElapsedTime = new TimeSpan();
            CompletedOn = DateTime.MinValue;
        }

        // Start or continue download
        public void Start()
        {
            if (this.Status == DownloadStatus.Initialized || this.Status == DownloadStatus.Paused
                || this.Status == DownloadStatus.Queued || this.HasError)
            {
                if (!this.SupportsRange && this.DownloadedSize > 0)
                {
                    this.StatusString = "Error: Server does not support resume";
                    this.HasError = true;
                    this.RaiseDownloadCompleted();
                    return;
                }

                this.HasError = false;
                this.Status = DownloadStatus.Waiting;
                RaisePropertyChanged("StatusString");

                if (DownloadManager.Instance.ActiveDownloads > Settings.Default.MaxDownloads)
                {
                    this.Status = DownloadStatus.Queued;
                    RaisePropertyChanged("StatusString");
                    return;
                }

                // Start the download thread
                DownloadThread = new Thread(new ThreadStart(DownloadFile));
                DownloadThread.IsBackground = true;
                DownloadThread.Start();
            }
        }

        // Pause download
        public void Pause()
        {
            if (this.Status == DownloadStatus.Waiting || this.Status == DownloadStatus.Downloading)
            {
                this.Status = DownloadStatus.Pausing;
            }
            if (this.Status == DownloadStatus.Queued)
            {
                this.Status = DownloadStatus.Paused;
                RaisePropertyChanged("StatusString");
            }
        }

        // Restart download
        public void Restart()
        {
            if (this.HasError || this.Status == DownloadStatus.Completed)
            {
                if (File.Exists(this.TempDownloadPath))
                {
                    File.Delete(this.TempDownloadPath);
                }
                if (File.Exists(this.DownloadPath))
                {
                    File.Delete(this.DownloadPath);
                }

                ResetProperties();
                this.Status = DownloadStatus.Waiting;
                UpdateDownloadDisplay();

                if (DownloadManager.Instance.ActiveDownloads > Settings.Default.MaxDownloads)
                {
                    this.Status = DownloadStatus.Queued;
                    RaisePropertyChanged("StatusString");
                    return;
                }

                DownloadThread = new Thread(new ThreadStart(DownloadFile));
                DownloadThread.IsBackground = true;
                DownloadThread.Start();
            }
        }

        // Download file bytes from the HTTP response stream
        private void DownloadFile()
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            ThrottledStream throttledStream = null;
            MemoryStream downloadCache = null;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            if (downloadRates.Count > 0)
                downloadRates.Clear();

            try
            {
                if (this.IsBatch && !this.BatchUrlChecked)
                {
                    CheckBatchUrl();
                    if (this.HasError)
                    {
                        this.RaiseDownloadCompleted();
                        return;
                    }
                    this.BatchUrlChecked = true;
                }

                if (!TempFileCreated)
                {
                    // Reserve local disk space for the file
                    CreateTempFile();
                    this.TempFileCreated = true;
                }

                this.lastStartTime = DateTime.UtcNow;

                if (this.Status == DownloadStatus.Waiting)
                    this.Status = DownloadStatus.Downloading;

                // Create request to the server to download the file
                webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
                webRequest.Method = "GET";

                if (this.ServerLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = this.ServerLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                if (this.Proxy != null)
                {
                    webRequest.Proxy = this.Proxy;
                }
                else
                {
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                }

                // Set download starting point
                webRequest.AddRange(DownloadedSize);

                // Get response from the server and the response stream
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                // Set a 5 second timeout, in case of internet connection break
                responseStream.ReadTimeout = 5000;

                // Set speed limit
                long maxBytesPerSecond = 0;
                if (Settings.Default.EnableSpeedLimit)
                {
                    maxBytesPerSecond = (long)((Settings.Default.SpeedLimit * 1024) / DownloadManager.Instance.ActiveDownloads);
                }
                else
                {
                    maxBytesPerSecond = ThrottledStream.Infinite;
                }
                throttledStream = new ThrottledStream(responseStream, maxBytesPerSecond);

                // Create memory cache with the specified size
                downloadCache = new MemoryStream(this.MaxCacheSize);

                // Create 1KB buffer
                byte[] downloadBuffer = new byte[this.BufferSize];

                int bytesSize = 0;
                CachedSize = 0;
                int receivedBufferCount = 0;

                // Download file bytes until the download is paused or completed
                while (true)
                {
                    if (SpeedLimitChanged)
                    {
                        if (Settings.Default.EnableSpeedLimit)
                        {
                            maxBytesPerSecond = (long)((Settings.Default.SpeedLimit * 1024) / DownloadManager.Instance.ActiveDownloads);
                        }
                        else
                        {
                            maxBytesPerSecond = ThrottledStream.Infinite;
                        }
                        throttledStream.MaximumBytesPerSecond = maxBytesPerSecond;
                        SpeedLimitChanged = false;
                    }

                    // Read data from the response stream and write it to the buffer
                    bytesSize = throttledStream.Read(downloadBuffer, 0, downloadBuffer.Length);

                    // If the cache is full or the download is paused or completed, write data from the cache to the temporary file
                    if (this.Status != DownloadStatus.Downloading || bytesSize == 0 || this.MaxCacheSize < CachedSize + bytesSize)
                    {
                        // Write data from the cache to the temporary file
                        WriteCacheToFile(downloadCache, CachedSize);

                        this.DownloadedSize += CachedSize;

                        // Reset the cache
                        downloadCache.Seek(0, SeekOrigin.Begin);
                        CachedSize = 0;

                        // Stop downloading the file if the download is paused or completed
                        if (this.Status != DownloadStatus.Downloading || bytesSize == 0)
                        {
                            break;
                        }
                    }

                    // Write data from the buffer to the cache
                    downloadCache.Write(downloadBuffer, 0, bytesSize);
                    CachedSize += bytesSize;

                    receivedBufferCount++;
                    if (receivedBufferCount == this.BufferCountPerNotification)
                    {
                        this.RaiseDownloadProgressChanged();
                        receivedBufferCount = 0;
                    }
                }

                // Update elapsed time when the download is paused or completed
                ElapsedTime = ElapsedTime.Add(DateTime.UtcNow - lastStartTime);

                // Change status
                if (this.Status != DownloadStatus.Deleting)
                {
                    if (this.Status == DownloadStatus.Pausing)
                    {
                        this.Status = DownloadStatus.Paused;
                        UpdateDownloadDisplay();
                    }
                    else if (this.Status == DownloadStatus.Queued)
                    {
                        UpdateDownloadDisplay();
                    }
                    else
                    {
                        this.CompletedOn = DateTime.UtcNow;
                        this.RaiseDownloadCompleted();
                    }
                }
            }
            catch (Exception ex)
            {
                // Show error in the status
                this.StatusString = "Error: " + ex.Message;
                this.HasError = true;
                this.RaiseDownloadCompleted();
            }
            finally
            {
                // Close the response stream and cache, stop the thread
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (throttledStream != null)
                {
                    throttledStream.Close();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                }
                if (downloadCache != null)
                {
                    downloadCache.Close();
                }
                if (DownloadThread != null)
                {
                    DownloadThread.Abort();
                }
            }
        }

        
        */
        #endregion

        /* 
         * 
         * MOVED TO ENUM FILE
        public static OSListRecordAction getDefaultActionFromOSListRecordStatus(OSListRecordStatus status)
        {
            if (status == OSListRecordStatus.Not_In_Library)
            {
                return OSListRecordAction.None;
            }
            return OSListRecordAction.None;
        }

        public static OSListRecordStatus? getNewOSListRecordStatusFromActionOption(OSListRecordAction action, OSListRecordStatus status)
        {
            if (action == OSListRecordAction.Add_To_Library && status == OSListRecordStatus.Not_In_Library)
            {
                return OSListRecordStatus.To_Be_Added;
            }
            if (action == OSListRecordAction.None && status == OSListRecordStatus.To_Be_Added )
            {
                return OSListRecordStatus.Not_In_Library;
            }
            else
            {
                return null;
            }
        }



        public static string getActionDescriptionFromActionEnum(OSListRecordAction actionEnum)
        {
            if (actionEnum == OSListRecordAction.None)
            {
                return "";
            }
            else
            {
                return actionEnum.ToString().Replace('_', ' ');
            }
        }

        public static OSListRecordAction getActionEnumFromActionDescription(string actionDescription)
        {
            if (actionDescription == null || actionDescription.Equals(""))
            {
                return OSListRecordAction.None;
            }
            else
            {
                return (OSListRecordAction)Enum.Parse(typeof(OSListRecordAction), actionDescription.Replace(' ', '_'), true);
            }
        }

        */


    }

}