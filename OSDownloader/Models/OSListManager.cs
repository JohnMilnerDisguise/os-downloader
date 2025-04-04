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
using OSDownloader.Models.JSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public ObservableCollection<OSListEntry> OSList = new ObservableCollection<OSListEntry>();

        //array of Wims in Customer's DeplyomentShare Library saved at last application close
        public ExistingDeploymentShareContents existingDeploymentShareContents = null;

        //array of OS info saved at last application close
        public OS[] osStateArrayFromLastClose = null;



        #endregion

        public static void AddOS(string uniqueIdentifier, string model, string name,
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable,
            string downloadsFolder
        )
        {
            OSListEntry preExistingOSRecordInList = Instance.OSList.FirstOrDefault( os => os.UniqueIdentifier != null && os.UniqueIdentifier.Equals(uniqueIdentifier) );
            if( preExistingOSRecordInList == null )
            {
                AddOS( uniqueIdentifier, new List<string> { model }, name,
                    osWimURL, osWimFileName, osWimFileSize,
                    bootWimURL, bootWimFileName, bootWimFileSize,
                    releaseNotes, publicVersionTable, downloadsFolder );
            }
            else if ( !preExistingOSRecordInList.ModelArray.Contains(model) )
            {
                preExistingOSRecordInList.ModelArray.Add(model);
            }
            //else do nothing
        }

        private static void AddOS(string uniqueIdentifier, List<string> models, string name, 
            string osWimURL, string osWimFileName, long osWimFileSize,
            string bootWimURL, string bootWimFileName, long bootWimFileSize,
            string releaseNotes, Public_Version_Table[] publicVersionTable,
            string downloadsFolder
        )
        {

            OSListEntry osListEntry = new OSListEntry(uniqueIdentifier, models, name, 
                osWimURL, osWimFileName, osWimFileSize,
                bootWimURL, bootWimFileName, bootWimFileSize,
                releaseNotes, publicVersionTable, null
            );

            OS osStateFromLastClose = ( Instance == null || Instance.osStateArrayFromLastClose == null) ? null : Array.Find( Instance.osStateArrayFromLastClose, o => o.unique_identifier == uniqueIdentifier );

            //load OS Status from last saved state
            if( osStateFromLastClose != null )
            {
                Enum.TryParse(osStateFromLastClose.status, out OSListRecordStatus lastStatusOnClose);
                osListEntry.Status = lastStatusOnClose;
                osListEntry.SelectedActionString = osStateFromLastClose.selected_action_string;
            }

            //OS.Wim Download Locations
            // Validate the URL (MOVE THIS TO WHEN DOWNLOAD IS SELECTED)
            osListEntry.DownloadClient_OSWim.CheckUrl();

            if (!osListEntry.DownloadClient_OSWim.HasError)
            {
                //calculate Temp File Path for OS Wim Download
                string osWimFilePath = Path.Combine(downloadsFolder, osListEntry.DownloadClient_OSWim.FileName);
                string osWimTempFilePath = osWimFilePath + ".tmp";
                osListEntry.DownloadClient_OSWim.TempDownloadPath = osWimTempFilePath;

                //check if the Last Close Saved State Info for this file is available and still valid
                bool osWimDownloadStateFromLastCloseIsStillValid = (
                    osStateFromLastClose != null &&
                    osListEntry.DownloadClient_OSWim.Url.OriginalString == osStateFromLastClose.os_wim_url &&
                    osListEntry.DownloadClient_OSWim.FileSize == osStateFromLastClose.os_wim_file_size &&
                    osListEntry.DownloadClient_OSWim.FileName + ".tmp" == osStateFromLastClose.os_wim_temp_file_name_only
                );

                //check if the File is already in the Customers DeplyomentShare OS Library with correct Name and Size
                Os_Wims preExistingOSWimInLibrary = (Instance == null || Instance.existingDeploymentShareContents == null) ? null : Array.Find<Os_Wims>( Instance.existingDeploymentShareContents.os_wims, o => o.file_name.ToUpper() == osListEntry.DownloadClient_OSWim.FileName.ToUpper() && o.file_size == osListEntry.DownloadClient_OSWim.FileSize );

                //SANITY CHECKING: DO a cleanup if the file is already in the Library but with the wrong size
                if ( preExistingOSWimInLibrary == null )
                {
                    //if not check for a file name match with no file size match and warn the user
                    Os_Wims preExistingOSWimInLibraryWithWrongSize = (Instance == null || Instance.existingDeploymentShareContents == null) ? null : Array.Find<Os_Wims>(Instance.existingDeploymentShareContents.os_wims, o => o.file_name.ToUpper() == osListEntry.DownloadClient_OSWim.FileName.ToUpper() );
                    if (preExistingOSWimInLibraryWithWrongSize != null)
                    {
                        string message = $"The OS .Wim file [{osWimFilePath}] is already in your Deployment Share OS Library but its file size [{preExistingOSWimInLibraryWithWrongSize.file_size}] is different to the expected file size of [{osListEntry.DownloadClient_OSWim.FileSize}].\n\nThe file will be downloaded again to ensure you have the latest copy.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, "OS Wim Already in Library with Different Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                //Load Previous State Data onto the OS Wim Download Client (or use default values if no state data available)
                //configure remaining fields to default value for a brand new downlaod
                osListEntry.DownloadClient_OSWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_OSWim.OpenFileOnCompletion = false;
                osListEntry.DownloadClient_OSWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Paused;
                //if the saved state for this object is available and saved valid, then use these values instead:
                if (osWimDownloadStateFromLastCloseIsStillValid)
                {
                    osListEntry.DownloadClient_OSWim.AddedOn = osStateFromLastClose.os_wim_added_on;
                    osListEntry.DownloadClient_OSWim.CompletedOn = osStateFromLastClose.os_wim_completed_on;
                    osListEntry.DownloadClient_OSWim.StatusText = osStateFromLastClose.os_wim_status_text;
                    osListEntry.DownloadClient_OSWim.ElapsedTime = osStateFromLastClose.os_wim_elapsed_time;
                }

                // Check if the final download file already exists
                bool completedDownloadAlreadyExistsInTempFolder = File.Exists(osWimFilePath);
                bool completedDownloadAlreadyExistsOnUSB = false;

                // Check if there is already an ongoing download on that path
                osListEntry.DownloadClient_OSWim.DownloadedSize = 0;
                if (File.Exists(osWimTempFilePath))
                {
                    if (completedDownloadAlreadyExistsInTempFolder || completedDownloadAlreadyExistsOnUSB || preExistingOSWimInLibrary != null)
                    {
                        //if the user already has the complete file then delete the temp file
                        try
                        {
                            File.Delete(osWimTempFilePath);
                            // Show warning of temp file cleanup to user
                            string message = $"A partially downloaded file was found at [{osWimTempFilePath}]\n\nThis file has been deleted because is not needed because you already have the complete file in your OS library.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, "Unneeded Partial Download Removed", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle any errors that occur during deletion
                            string errorMessage = $"A partially downloaded file was found at [{osWimTempFilePath}]\n\nThis file is no longer needed because you already have the complete file in your OS library, however the Deletion Failed with the following error:\n\n{ex.Message}";
                            Xceed.Wpf.Toolkit.MessageBox.Show(errorMessage, "Error Cleaning Up Partial Download", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        //else keep the temp file and attempt to restart the download from where it was last left off
                        osListEntry.DownloadClient_OSWim.TempFileCreated = true;
                        if (osWimDownloadStateFromLastCloseIsStillValid)
                        {
                            osListEntry.DownloadClient_OSWim.DownloadedSize = osStateFromLastClose.os_wim_downloaded_size;
                        }
                        else
                        {
                            osListEntry.DownloadClient_OSWim.DownloadedSize = 0;
                            string message = $"A part-downloaded Temp OS .Wim file was found [{osWimTempFilePath}] but the information for how much of the download has completed is lost. The Download will Restart again from the beginning";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, "Part Completed Download must be Restarted", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }



                // Check if the actual download file already exists and flag it as completed if so
                if (completedDownloadAlreadyExistsInTempFolder || completedDownloadAlreadyExistsOnUSB || preExistingOSWimInLibrary != null)
                {
                    //string message = $"The OS .Wim download is already complete: [{osWimFilePath}]";
                    //MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "OS Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    osListEntry.DownloadClient_OSWim.CompletedOn = DateTime.UtcNow;
                    osListEntry.DownloadClient_OSWim.Status = DownloadStatus.Completed;
                    osListEntry.DownloadClient_OSWim.DownloadedSize = osListEntry.DownloadClient_OSWim.FileSize;
                }

            }

            // Validate the URL (MOVE THIS TO WHEN DOWNLOAD IS SELECTED)
            osListEntry.DownloadClient_BootWim.CheckUrl();

            if (!osListEntry.DownloadClient_BootWim.HasError)
            {
                //calculate Temp File Path for Boot Wim Download
                string bootWimFilePath = Path.Combine(downloadsFolder, osListEntry.DownloadClient_BootWim.FileName);
                string bootWimTempFilePath = bootWimFilePath + ".tmp";
                osListEntry.DownloadClient_BootWim.TempDownloadPath = bootWimTempFilePath;

                bool bootWimDownloadStateFromLastCloseIsStillValid = (
                    osStateFromLastClose != null &&
                    osListEntry.DownloadClient_BootWim.Url.OriginalString == osStateFromLastClose.boot_wim_url &&
                    osListEntry.DownloadClient_BootWim.FileSize == osStateFromLastClose.boot_wim_file_size &&
                    osListEntry.DownloadClient_BootWim.FileName + ".tmp" == osStateFromLastClose.boot_wim_temp_file_name_only
                );

                // Check if there is already an ongoing download on that path
                osListEntry.DownloadClient_BootWim.DownloadedSize = 0;
                if (File.Exists(bootWimTempFilePath))
                {
                    osListEntry.DownloadClient_BootWim.TempFileCreated = true;
                    if (bootWimDownloadStateFromLastCloseIsStillValid)
                    {
                        osListEntry.DownloadClient_BootWim.DownloadedSize = osStateFromLastClose.boot_wim_downloaded_size;
                    }
                    else
                    {
                        osListEntry.DownloadClient_BootWim.DownloadedSize = 0;
                        string message = $"A part-downloaded Temp Boot .Wim file was found [{bootWimTempFilePath}] but the information for how much of the download has completed is lost. The Download will Restart again from the beginning";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, "Part Completed Download must be Restarted", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                //configure remaining fields to default value for a brand new downlaod
                osListEntry.DownloadClient_BootWim.AddedOn = DateTime.UtcNow;
                osListEntry.DownloadClient_BootWim.OpenFileOnCompletion = false;
                osListEntry.DownloadClient_BootWim.CompletedOn = DateTime.MinValue;
                osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Paused;

                //if the saved state for this object is available and saved valid, then use these values instead:
                if (bootWimDownloadStateFromLastCloseIsStillValid)
                {
                    osListEntry.DownloadClient_BootWim.AddedOn = osStateFromLastClose.boot_wim_added_on;
                    osListEntry.DownloadClient_BootWim.CompletedOn = osStateFromLastClose.boot_wim_completed_on;
                    osListEntry.DownloadClient_BootWim.StatusText = osStateFromLastClose.boot_wim_status_text;
                    osListEntry.DownloadClient_BootWim.ElapsedTime = osStateFromLastClose.boot_wim_elapsed_time;
                }

                // Check if the actual download file already exists and flag it as completed if so
                if (File.Exists(bootWimFilePath))
                {
                    //string message = $"The Boot .Wim download is already complete: [{bootWimFilePath}]";
                    //MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "Boot Wim Already Downloaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    osListEntry.DownloadClient_BootWim.CompletedOn = DateTime.UtcNow;
                    osListEntry.DownloadClient_BootWim.Status = DownloadStatus.Completed;
                    osListEntry.DownloadClient_BootWim.DownloadedSize = osListEntry.DownloadClient_BootWim.FileSize;
                }

            }

            //Set Status to NOT Valid if there was a validation error
            osListEntry.Status = osListEntry.HasError ? OSListRecordStatus.NOT_VALID : osListEntry.Status;

            //If status is still on the default, untouched value 'Initialized' then set it to 'Not Included'
            osListEntry.Status = osListEntry.Status == OSListRecordStatus.Initialized ? OSListRecordStatus.Not_In_Library : osListEntry.Status;

            // Add the OS and it's two download clients to the OS list
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
        

        public void SaveFullStateToJSON()
        {
            // Pause downloads
            SetAllActiveOSRecordsToPaused();

            StateAtLastClose stateAtLastClose = new StateAtLastClose
            {
                osses = new Osses
                {
                    os = new OS[OSList.Count]
                }
            };

            for (int i = 0; i < OSList.Count; i++) 
            {
                OSListEntry os = OSList[i];
                /*
                if (os.DownloadClient_BootWim.Status != DownloadStatus.Paused &&
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
                */

                stateAtLastClose.osses.os[i] = new OS()
                {
                    unique_identifier = os.UniqueIdentifier,
                    selected_action_string = os.SelectedActionString,
                    status = os.Status.ToString(),
                    boot_wim_url = os.DownloadClient_BootWim.Url.OriginalString,
                    boot_wim_temp_path = os.DownloadClient_BootWim.TempDownloadPath,
                    boot_wim_temp_file_name_only = System.IO.Path.GetFileName(os.DownloadClient_BootWim.TempDownloadPath),
                    boot_wim_file_size = os.DownloadClient_BootWim.FileSize,
                    boot_wim_downloaded_size = os.DownloadClient_BootWim.DownloadedSize,
                    boot_wim_status = os.DownloadClient_BootWim.Status.ToString(),
                    boot_wim_status_text = os.DownloadClient_BootWim.StatusText,
                    boot_wim_elapsed_time = os.DownloadClient_BootWim.TotalElapsedTime,
                    boot_wim_added_on = os.DownloadClient_BootWim.AddedOn,
                    boot_wim_completed_on = os.DownloadClient_BootWim.CompletedOn,
                    boot_wim_has_error = os.DownloadClient_BootWim.HasError,
                    os_wim_url = os.DownloadClient_OSWim.Url.OriginalString,
                    os_wim_temp_path = os.DownloadClient_OSWim.TempDownloadPath,
                    os_wim_temp_file_name_only = System.IO.Path.GetFileName(os.DownloadClient_OSWim.TempDownloadPath),
                    os_wim_file_size = os.DownloadClient_OSWim.FileSize,
                    os_wim_downloaded_size = os.DownloadClient_OSWim.DownloadedSize,
                    os_wim_status = os.DownloadClient_OSWim.Status.ToString(),
                    os_wim_status_text = os.DownloadClient_OSWim.StatusText,
                    os_wim_elapsed_time = os.DownloadClient_OSWim.TotalElapsedTime,
                    os_wim_added_on = os.DownloadClient_OSWim.AddedOn,
                    os_wim_completed_on = os.DownloadClient_OSWim.CompletedOn,
                    os_wim_has_error = os.DownloadClient_OSWim.HasError
                };
            }

            // Serialize the object to JSON
            string json = JsonConvert.SerializeObject(stateAtLastClose, Formatting.Indented);

            // Save downloads to json file
            string configDirectoryPath = GetApplicationsConfigDirectoryPath();
            string StateAtLastCloseJSONPath = System.IO.Path.Combine(configDirectoryPath, "stateAtLastClose.json");
            File.WriteAllText(StateAtLastCloseJSONPath, json);
        }
        public void LoadFullStateFromJSON()
        {
            string configDirectoryPath = GetApplicationsConfigDirectoryPath();
            string StateAtLastCloseJSONPath = System.IO.Path.Combine(configDirectoryPath, "stateAtLastClose.json");

            try
            {
                if (osStateArrayFromLastClose == null && File.Exists(StateAtLastCloseJSONPath))
                {

                    // Step 1: Read the JSON file into a string
                    string json = File.ReadAllText(StateAtLastCloseJSONPath);

                    // Step 2: Deserialize the JSON string into a StateAtLastClose object
                    var stateAtLastClose = JsonConvert.DeserializeObject<StateAtLastClose>(json, new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter>
                        {
                            new JSON.TimeSpanConverter(), // Use the custom TimeSpanConverter
                            new IsoDateTimeConverter() // Use ISO 8601 format for DateTime
                        }
                    });

                    if (stateAtLastClose != null && stateAtLastClose.osses != null && stateAtLastClose.osses.os != null)
                    {
                        foreach (OS os in stateAtLastClose.osses.os)
                        {
                            /*
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
                            */
                        }

                        // Replace the file with an empty json file
                        StateAtLastClose blankStateAtLastCloseObject = new StateAtLastClose
                        {
                            osses = new Osses
                            {
                                os = new OS[] { }
                            }
                        };
                        // Serialize the blank state object to JSON
                        string blankStateJson = JsonConvert.SerializeObject(blankStateAtLastCloseObject, Formatting.Indented);
                        // Save blank state JSON back to overwrite file
                        File.WriteAllText(StateAtLastCloseJSONPath, blankStateJson);

                        osStateArrayFromLastClose = stateAtLastClose.osses.os;
                    }
                }
            }
            catch (Exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("There was an error while loading the download list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadExistingDeploymentShareContentsFromJSON()
        {
            string configDirectoryPath = GetApplicationsConfigDirectoryPath();
            string ExistingDeploymentShareContentsJSONPath = System.IO.Path.Combine(configDirectoryPath, "existingDeploymentShareContents.json");

            try
            {
                if (existingDeploymentShareContents == null )
                {
                    if (File.Exists(ExistingDeploymentShareContentsJSONPath))
                    {
                        // Step 1: Read the JSON file into a string
                        string json = File.ReadAllText(ExistingDeploymentShareContentsJSONPath);

                        // Step 2: Deserialize the JSON string into a StateAtLastClose object
                        existingDeploymentShareContents = JsonConvert.DeserializeObject<ExistingDeploymentShareContents>(json);
                    }
                    else
                    {
                        existingDeploymentShareContents = new ExistingDeploymentShareContents()
                        {
                            boot_wims = new Boot_Wims[] { },
                            os_wims = new Os_Wims[] { }
                        };
                        Xceed.Wpf.Toolkit.MessageBox.Show("Could not ascertain what OS files, if any, that you already have saved in your Deployment Share OS Library\n\nAssuming your Library is empty. If this is not the case please re-make your OS Downloader USB from OS Manager", "Missing Existing Library Contents Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("There was an error while loading the download list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
