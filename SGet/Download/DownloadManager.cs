using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;

namespace SGet
{
    public class DownloadManager
    {
        // Class instance, used to access non-static members
        private static DownloadManager instance = new DownloadManager();

        public static DownloadManager Instance
        {
            get
            {
                return instance;
            }
        }

        public static WebDownloadClient findDownloadInstanceByURLFromDownloadsList(string url)
        {
            WebDownloadClient foundDownload = null;
            Instance.DownloadsListByURL.TryGetValue(url.ToUpper().Trim(), out foundDownload);
            return foundDownload;
        }

        public static WebDownloadClient findDownloadInstanceByURLFromAllDownloadClientStore( string url )
        {
            WebDownloadClient foundDownload = null;
            Instance.AllDownloadClientStoreByURL.TryGetValue(url.ToUpper().Trim(), out foundDownload);
            return foundDownload;
        }

        public static void AddToDownloadsListIfNotAlreadyInList(WebDownloadClient clientObjectFromStore)
        {
            if ( ! Instance.DownloadsList.Contains(clientObjectFromStore) )
            {
                Instance.DownloadsList.Add(clientObjectFromStore);
                clientObjectFromStore.Status = DownloadStatus.Paused;
                Instance.DownloadsListByURL.Add(clientObjectFromStore.Url.OriginalString.ToUpper().Trim(), clientObjectFromStore);
                //clientObjectFromStore.Start();
            }
        }

        public static void RemoveFromDownloadsListIfAlreadyInList(WebDownloadClient clientObjectFromStore)
        {
            if ( Instance.DownloadsList.Contains( clientObjectFromStore ) )
            {
                Instance.DownloadsList.Remove( clientObjectFromStore );
                clientObjectFromStore.Status = DownloadStatus.Paused;
                Instance.DownloadsListByURL.Remove( clientObjectFromStore.Url.OriginalString.ToUpper().Trim() );
            }
        }

        public static void AddToAllDownloadClientStore(WebDownloadClient clientObjectFromStore)
        {
            Instance.AllDownloadClientStore.Add( clientObjectFromStore );
            Instance.AllDownloadClientStoreByURL.Add(clientObjectFromStore.Url.OriginalString.ToUpper().Trim(), clientObjectFromStore);
        }

        private static NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        // Collection which contains all download clients, bound to the DataGrid control
        public ObservableCollection<WebDownloadClient> DownloadsList = new ObservableCollection<WebDownloadClient>();  //just the ones to appear in the grid
        public Dictionary<string, WebDownloadClient> DownloadsListByURL = new Dictionary<string, WebDownloadClient>();

        public ObservableCollection<WebDownloadClient> AllDownloadClientStore = new ObservableCollection<WebDownloadClient>(); //all of them for all osses even unselected ones
        public Dictionary<string, WebDownloadClient> AllDownloadClientStoreByURL = new Dictionary<string, WebDownloadClient>();
        
        #region Properties

        // Number of currently active downloads
        public int ActiveDownloads
        {
            get
            {
                int active = 0;
                foreach (WebDownloadClient d in DownloadsList)
                {
                    if (!d.HasError)
                        if (d.Status == DownloadStatus.Waiting || d.Status == DownloadStatus.Downloading)
                            active++;
                }
                return active;
            }
        }

        // Number of completed downloads
        public int CompletedDownloads
        {
            get
            {
                int completed = 0;
                foreach (WebDownloadClient d in DownloadsList)
                {
                    if (d.Status == DownloadStatus.Completed)
                        completed++;
                }
                return completed;
            }
        }

        // Total number of downloads in the list
        public int TotalDownloads
        {
            get
            {
                return DownloadsList.Count;
            }
        }

        #endregion

        #region Methods

        // Format file size or downloaded size string
        public static string FormatSizeString(long byteSize)
        {
            double kiloByteSize = (double)byteSize / 1024D;
            double megaByteSize = kiloByteSize / 1024D;
            double gigaByteSize = megaByteSize / 1024D;

            if (byteSize < 1024)
                return String.Format(numberFormat, "{0} B", byteSize);
            else if (byteSize < 1048576)
                return String.Format(numberFormat, "{0:0.00} kB", kiloByteSize);
            else if (byteSize < 1073741824)
                return String.Format(numberFormat, "{0:0.00} MB", megaByteSize);
            else
                return String.Format(numberFormat, "{0:0.00} GB", gigaByteSize);
        }

        // Format download speed string
        public static string FormatSpeedString(int speed)
        {
            float kbSpeed = (float)speed / 1024F;
            float mbSpeed = kbSpeed / 1024F;

            if (speed <= 0)
                return String.Empty;
            else if (speed < 1024)
                return speed.ToString() + " B/s";
            else if (speed < 1048576)
                return kbSpeed.ToString("#.00", numberFormat) + " kB/s";
            else
                return mbSpeed.ToString("#.00", numberFormat) + " MB/s";
        }

        // Format time span string so it can display values of more than 24 hours
        public static string FormatTimeSpanString(TimeSpan span)
        {
            string hours = ((int)span.TotalHours).ToString();
            string minutes = span.Minutes.ToString();
            string seconds = span.Seconds.ToString();
            if ((int)span.TotalHours < 10)
                hours = "0" + hours;
            if (span.Minutes < 10)
                minutes = "0" + minutes;
            if (span.Seconds < 10)
                seconds = "0" + seconds;

            return String.Format("{0}:{1}:{2}", hours, minutes, seconds);
        }

        #endregion

        #region Event Handlers
        public static void OSListEntryStatusChanged(object sender, EventArgs e)
        {
            // Start the first download in the queue, if it exists
            OSListEntry osRecord = (OSListEntry)sender;
            if( OSListRecordEnumUtils.shouldOSListFilesAppearInDownloadList( osRecord.Status ) )
            {
                AddToDownloadsListIfNotAlreadyInList( osRecord.DownloadClient_OSWim   );
                AddToDownloadsListIfNotAlreadyInList (osRecord.DownloadClient_BootWim );
            }
            else
            {
                RemoveFromDownloadsListIfAlreadyInList(osRecord.DownloadClient_OSWim);
                RemoveFromDownloadsListIfAlreadyInList(osRecord.DownloadClient_BootWim);
            }
        }
        #endregion
    }
}
