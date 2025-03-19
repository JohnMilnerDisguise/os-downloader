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
using OSDownloader.Properties;
using System.Net;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Windows.Shapes;
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
            string releaseNotes, Public_Version_Table[] publicVersionTable, 
            OSDownloader.MainWindow mainWindow,
            string downloadsFolder
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
                osListEntry.DownloadClient_OSWim.TempDownloadPath = osWimTempFilePath;
                if (File.Exists(osWimTempFilePath))
                {
                    FileInfo osWimTempFileInfoObject = new System.IO.FileInfo(osWimTempFilePath);
                    osListEntry.DownloadClient_OSWim.DownloadedSize = osWimTempFileInfoObject.Length;
                    string message = $"There is already a part-completed OS .Wim download at the path [{osWimTempFilePath}]. Setting Clients DownloadedSize to {osListEntry.DownloadClient_OSWim.DownloadedSize} Bytes";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "Part Completed Download Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // Check if the actual download file already exists
                osListEntry.DownloadClient_OSWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Paused;
                if (File.Exists(osWimFilePath))
                {
                    string message = $"The OS .Wim download is already complete: [{osWimFilePath}]";
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "OS Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    osListEntry.DownloadClient_OSWim.CompletedOn = DateTime.UtcNow;
                    osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Completed;
                }

                //configure remaining fields
                osListEntry.DownloadClient_OSWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_OSWim.OpenFileOnCompletion = false;
                
            }

            if (!osListEntry.DownloadClient_BootWim.HasError)
            {
                //OS.Wim Download Locations
                string bootWimFilePath = Path.Combine(downloadsFolder, osListEntry.DownloadClient_BootWim.FileName);
                string bootWimTempFilePath = bootWimFilePath + ".tmp";
                // Check if there is already an ongoing download on that path
                osListEntry.DownloadClient_BootWim.TempDownloadPath = bootWimTempFilePath;
                if (File.Exists(bootWimTempFilePath))
                {
                    FileInfo osWimTempFileInfoObject = new System.IO.FileInfo(bootWimTempFilePath);
                    osListEntry.DownloadClient_BootWim.DownloadedSize = bootWimTempFilePath.Length;
                    string message = $"There is already a part-completed Boot .Wim download in progress at the path [{bootWimTempFilePath}]. Setting Clients DownloadedSize to {osListEntry.DownloadClient_BootWim.DownloadedSize} Bytes";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "File Download Location Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Check if the actual download file already exists
                osListEntry.DownloadClient_BootWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Paused;
                if (File.Exists(bootWimFilePath))
                {
                    string message = $"The Boot .Wim download is already complete: [{bootWimFilePath}]";
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "Boot Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    osListEntry.DownloadClient_BootWim.CompletedOn = DateTime.UtcNow;
                    osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Completed;
                }
                // Validate the URL (MOVE THIS TO WHEN DOWNLOAD IS SELECTED)
                osListEntry.DownloadClient_BootWim.CheckUrl();
                if (osListEntry.DownloadClient_BootWim.HasError)
                    return;
                //configure remaining fields
                osListEntry.DownloadClient_BootWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_BootWim.OpenFileOnCompletion = false;
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
            ObservableCollection<OSListEntry> selectedOSRecords = new ObservableCollection<OSListEntry>(OSList.Where((osRecord) => osRecord.Status == OSListRecordStatus.Active ) );
            return selectedOSRecords;
        }

        public void SetAllActiveOSRecordsToPaused()
        {
            foreach (OSListEntry osListEntry in Instance.getUnPausedOSRecords())
            {
                if (osListEntry.Status != OSListRecordStatus.Paused)
                {
                    osListEntry.Pause();
                }
            }
        }

        public static string GetApplicationsConfigDirectoryPath()
        {
            string exesDirectoryPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string configDirectoryPath = System.IO.Path.Combine(exesDirectoryPath, "config");
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }
            return configDirectoryPath;
        }
        

        public void SaveDownloadsToXml()
        {
            // Pause downloads
            SetAllActiveOSRecordsToPaused();

            XElement root = new XElement("osses");

            foreach (OSListEntry os in OSList)
            {
                if( os.DownloadClient_BootWim.Status != DownloadStatus.Paused &&
                    os.DownloadClient_BootWim.Status != DownloadStatus.Initialized &&
                    os.DownloadClient_BootWim.Status != DownloadStatus.Error &&
                    os.DownloadClient_BootWim.Status != DownloadStatus.Completed )
                {
                    string message = $"The Boot Wim for {os.UniqueIdentifier} has not yet fully paused, it's status is currently {os.DownloadClient_BootWim.StatusString}";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "Download Not Paused", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                if (os.DownloadClient_OSWim.Status != DownloadStatus.Paused &&
                    os.DownloadClient_OSWim.Status != DownloadStatus.Initialized &&
                    os.DownloadClient_OSWim.Status != DownloadStatus.Error &&
                    os.DownloadClient_OSWim.Status != DownloadStatus.Completed)
                {
                    string message = $"The OS Wim for {os.UniqueIdentifier} has not yet fully paused, it's status is currently {os.DownloadClient_OSWim.StatusString}";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "Download Not Paused", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                XElement xdl = new XElement("os",
                    new XElement("unique_identifier", os.UniqueIdentifier),
                    new XElement("selected_action_string", os.SelectedActionString),
                    new XElement("status", os.Status),
                    new XElement("boot_wim_url", os.DownloadClient_BootWim.Url.OriginalString),
                    new XElement("boot_wim_temp_path", os.DownloadClient_BootWim.TempDownloadPath),
                    new XElement("boot_wim_file_size", os.DownloadClient_BootWim.FileSize.ToString()),
                    new XElement("boot_wim_downloaded_size", os.DownloadClient_BootWim.DownloadedSize.ToString()),
                    new XElement("boot_wim_status", os.DownloadClient_BootWim.Status.ToString()),
                    new XElement("boot_wim_status_text", os.DownloadClient_BootWim.StatusText),
                    new XElement("boot_wim_total_time", os.DownloadClient_BootWim.TotalElapsedTime.ToString()),
                    new XElement("boot_wim_added_on", os.DownloadClient_BootWim.AddedOn.ToString()),
                    new XElement("boot_wim_completed_on", os.DownloadClient_BootWim.CompletedOn.ToString()),
                    new XElement("boot_wim_has_error", os.DownloadClient_BootWim.HasError.ToString()),
                    new XElement("os_wim_url", os.DownloadClient_OSWim.Url.OriginalString),
                    new XElement("os_wim_temp_path", os.DownloadClient_OSWim.TempDownloadPath),
                    new XElement("os_wim_file_size", os.DownloadClient_OSWim.FileSize.ToString()),
                    new XElement("os_wim_downloaded_size", os.DownloadClient_OSWim.DownloadedSize.ToString()),
                    new XElement("os_wim_status", os.DownloadClient_OSWim.Status.ToString()),
                    new XElement("os_wim_status_text", os.DownloadClient_OSWim.StatusText),
                    new XElement("os_wim_total_time", os.DownloadClient_OSWim.TotalElapsedTime.ToString()),
                    new XElement("os_wim_added_on", os.DownloadClient_OSWim.AddedOn.ToString()),
                    new XElement("os_wim_completed_on", os.DownloadClient_OSWim.CompletedOn.ToString()),
                    new XElement("os_wim_has_error", os.DownloadClient_OSWim.HasError.ToString())
                );
                root.Add(xdl);
            }

            XDocument xd = new XDocument();
            xd.Add(root);
            // Save downloads to XML file
            string configDirectoryPath = GetApplicationsConfigDirectoryPath();
            string StateAtLastCloseXMLPath = System.IO.Path.Combine(configDirectoryPath, "StateAtLastClose.xml");
            xd.Save(StateAtLastCloseXMLPath);
        }
        /*
        private void LoadDownloadsFromXml()
        {
            try
            {
                if (File.Exists("Downloads.xml"))
                {
                    // Load downloads from XML file
                    XElement downloads = XElement.Load("Downloads.xml");
                    if (downloads.HasElements)
                    {
                        IEnumerable<XElement> downloadsList =
                            from el in downloads.Elements()
                            select el;
                        foreach (XElement download in downloadsList)
                        {
                            // Create WebDownloadClient object based on XML data
                            WebDownloadClient downloadClient = new WebDownloadClient(download.Element("url").Value);

                            downloadClient.FileName = download.Element("file_name").Value;

                            downloadClient.DownloadProgressChanged += downloadClient.DownloadProgressChangedHandler;
                            downloadClient.DownloadCompleted += downloadClient.DownloadCompletedHandler;
                            downloadClient.PropertyChanged += this.PropertyChangedHandler;
                            downloadClient.StatusChanged += this.StatusChangedHandler;
                            downloadClient.DownloadCompleted += this.DownloadCompletedHandler;

                            string username = download.Element("username").Value;
                            string password = download.Element("password").Value;
                            if (username != String.Empty && password != String.Empty)
                            {
                                downloadClient.ServerLogin = new NetworkCredential(username, password);
                            }

                            downloadClient.TempDownloadPath = download.Element("temp_path").Value;
                            downloadClient.FileSize = Convert.ToInt64(download.Element("file_size").Value);
                            downloadClient.DownloadedSize = Convert.ToInt64(download.Element("downloaded_size").Value);

                            DownloadManager.Instance.DownloadsList.Add(downloadClient);

                            if (download.Element("status").Value == "Completed")
                            {
                                downloadClient.Status = DownloadStatus.Completed;
                            }
                            else
                            {
                                downloadClient.Status = DownloadStatus.Paused;
                            }

                            downloadClient.StatusText = download.Element("status_text").Value;

                            downloadClient.ElapsedTime = TimeSpan.Parse(download.Element("total_time").Value);
                            downloadClient.AddedOn = DateTime.Parse(download.Element("added_on").Value);
                            downloadClient.CompletedOn = DateTime.Parse(download.Element("completed_on").Value);

                            downloadClient.SupportsRange = Boolean.Parse(download.Element("supports_resume").Value);
                            downloadClient.HasError = Boolean.Parse(download.Element("has_error").Value);
                            downloadClient.OpenFileOnCompletion = Boolean.Parse(download.Element("open_file").Value);
                            downloadClient.TempFileCreated = Boolean.Parse(download.Element("temp_created").Value);
                            downloadClient.IsBatch = Boolean.Parse(download.Element("is_batch").Value);
                            downloadClient.BatchUrlChecked = Boolean.Parse(download.Element("url_checked").Value);

                            if (downloadClient.Status == DownloadStatus.Paused && !downloadClient.HasError && Settings.Default.StartDownloadsOnStartup)
                            {
                                downloadClient.Start();
                            }
                        }

                        // Create empty XML file
                        XElement root = new XElement("downloads");
                        XDocument xd = new XDocument();
                        xd.Add(root);
                        xd.Save("Downloads.xml");
                    }
                }
            }
            catch (Exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("There was an error while loading the download list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */


    }
}
