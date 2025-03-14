using OSDownloader;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Runtime.CompilerServices;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
//using static System.Net.WebRequestMethods;

namespace OSDownloader.Models
{
    public class OSListManager
    {

        // Class instance, used to access non-static members
        private static OSListManager instance = new OSListManager();

        public static OSListManager Instance
        {
            get
            {
                return instance;
            }
        }

        private static NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        #region Properties

        // Collection which contains all download clients, bound to the DataGrid control
        //public ObservableCollection<OSListEntry> OSList = new ObservableCollection<OSListEntry>();
        public ObservableCollection<OSListEntry> OSList = new ObservableCollection<OSListEntry>();

        #endregion

        public static void AddOS(string uniqueIdentifier, string model, string name,
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable, OSDownloader.MainWindow mainWindow, string downloadsFolder
        )
        {
            OSListEntry preExistingOSRecordInList = Instance.OSList.FirstOrDefault( os => os.UniqueIdentifier != null && os.UniqueIdentifier.Equals(uniqueIdentifier) );
            if( preExistingOSRecordInList == null )
            {
                AddOS( uniqueIdentifier, new List<string> { model }, name,
                    osWimURL, osWimFileName, osWimFileSize,
                    bootWimURL, bootWimFileName, bootWimFileSize,
                    releaseNotes, publicVersionTable, mainWindow, downloadsFolder );
            }
            else if (!preExistingOSRecordInList.ModelArray.Contains(model) )
            {
                preExistingOSRecordInList.ModelArray.Add(model);
            }
            //else do nothing
        }

        private static void AddOS(string uniqueIdentifier, List<string> models, string name, 
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable, OSDownloader.MainWindow mainWindow, string downloadsFolder
        )
        {

            OSListEntry osListEntry = new OSListEntry(uniqueIdentifier, models, name, 
                osWimURL, osWimFileName, osWimFileSize,
                bootWimURL, bootWimFileName, bootWimFileSize,
                releaseNotes, publicVersionTable, mainWindow
            );

            // Register WebDownloadClient events
            //download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
            //download.DownloadCompleted += download.DownloadCompletedHandler;
            //download.DownloadCompleted += mainWindow.DownloadCompletedHandler;

            //OS.Wim Download Locations
            // Validate the URL (MOVE THIS TO WHEN DOWNLOAD IS SELECTED)
            osListEntry.DownloadClient_OSWim.CheckUrl();
            if (!osListEntry.DownloadClient_OSWim.HasError)
            {
                string osWimFilePath = Path.Combine(downloadsFolder, osListEntry.DownloadClient_OSWim.FileName);
                string osWimTempFilePath = osWimFilePath + ".tmp";
                // Check if there is already an ongoing download on that path
                if (File.Exists(osWimTempFilePath))
                {
                    string message = $"There is already a download in progress at the path [{osWimTempFilePath}]";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "File Download Location Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Check if the actual download file already exists
                if (File.Exists(osWimFilePath))
                {
                    osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Completed;
                    string message = $"The OS .Wim download is already complete: [{osWimFilePath}]";
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "OS Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                //configure remaining fields
                osListEntry.DownloadClient_OSWim.TempDownloadPath = osWimTempFilePath;
                osListEntry.DownloadClient_OSWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_OSWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_OSWim.OpenFileOnCompletion = false;
                osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Paused;
            }

            if (!osListEntry.DownloadClient_BootWim.HasError)
            {
                //OS.Wim Download Locations
                string bootWimFilePath = Path.Combine(downloadsFolder, osListEntry.DownloadClient_BootWim.FileName);
                string bootWimTempFilePath = bootWimFilePath + ".tmp";
                // Check if there is already an ongoing download on that path
                if (File.Exists(bootWimTempFilePath))
                {
                    string message = $"There is already a download in progress at the path [{bootWimTempFilePath}]";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "File Download Location Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Check if the actual download file already exists
                if (File.Exists(bootWimFilePath))
                {
                    osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Completed;
                    string message = $"The Boot .Wim download is already complete: [{bootWimFilePath}]";
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "Boot Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // Validate the URL (MOVE THIS TO WHEN DOWNLOAD IS SELECTED)
                osListEntry.DownloadClient_BootWim.CheckUrl();
                if (osListEntry.DownloadClient_BootWim.HasError)
                    return;
                //configure remaining fields
                osListEntry.DownloadClient_BootWim.TempDownloadPath = bootWimTempFilePath;
                osListEntry.DownloadClient_BootWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_BootWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_BootWim.OpenFileOnCompletion = false;
                osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Paused;
            }

            //Set Status Depending On Whether OS in library
            osListEntry.Status = osListEntry.HasError ? OSListRecordStatus.NOT_VALID : OSListRecordStatus.Not_In_Library;


            //osListEntry.ReleaseNotes = $"Hello\nThese are the Release Notes for {name}";

            // Add the download to the downloads list
            OSListManager.Instance.OSList.Add(osListEntry);
        }

        public ObservableCollection<OSListEntry> getPausedOSRecords()
        {
            ObservableCollection<OSListEntry> selectedOSRecords = new ObservableCollection<OSListEntry>( OSList.Where((osRecord) => osRecord.Status == OSListRecordStatus.To_Be_Added || osRecord.Status == OSListRecordStatus.Paused) );
            return selectedOSRecords;
        }

        public ObservableCollection<OSListEntry> getUnPausedOSRecords()
        {
            ObservableCollection<OSListEntry> selectedOSRecords = new ObservableCollection<OSListEntry>(OSList.Where((osRecord) => osRecord.Status == OSListRecordStatus.Active || osRecord.Status == OSListRecordStatus.Downloading || osRecord.Status == OSListRecordStatus.Queued ) );
            return selectedOSRecords;
        }

    }
}
