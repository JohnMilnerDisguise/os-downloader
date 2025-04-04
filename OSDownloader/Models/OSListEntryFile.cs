using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace OSDownloader.Models
{
    public class OSListEntryFile
    {
        #region Fields and Properties

        public WebDownloadClient DownloadClient { get; set; }

        public string FinalDestinationPath;

        public bool tempFileExists;
        public bool finishedDownloadFileExists;
        public bool finalDestinationFileExists;

        
        #endregion

        #region Constructor and Events

        public OSListEntryFile( OSListEntry parentOSListEntry, string fileTypeDescription, string downloadURL, long fileSize, string fileName )
        {



            //Initialize the the Download Client for this File
            WebDownloadClient existingWebDownloadClient = DownloadManager.findDownloadInstanceByURLFromAllDownloadClientStore(downloadURL);
            if (existingWebDownloadClient == null)
            {
                //Sanity Check Data from the API
                if (fileName == null || fileName.Trim().Length == 0)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"The {fileTypeDescription} File Name for the {parentOSListEntry.UniqueIdentifier} OS is Missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DownloadClient.HasError = true;
                }

                //initialize CLient Properties
                this.DownloadClient = new WebDownloadClient(downloadURL);
                this.DownloadClient.FileSize = fileSize;
                this.DownloadClient.FileName = fileName;

                //Wire Up Event Handlers Radiating off the Download Client
                this.DownloadClient.DownloadProgressChanged += this.DownloadClient.DownloadProgressChangedHandler;
                this.DownloadClient.DownloadCompleted += this.DownloadClient.DownloadCompletedHandler;
                this.DownloadClient.StatusChanged += DownloadManager.HandleDownloadEntryStatusChanged;
                this.DownloadClient.StatusChanged += parentOSListEntry.HandleDownloadClientStatusChanged;
                //this.DownloadClient.DownloadCompleted += parentOSListEntry.DownloadCompletedHandler;
                this.DownloadClient.PropertyChanged += HandleDownloadClientPropertyChanged;

                DownloadManager.Instance.AllDownloadClientStore.Add(this.DownloadClient);
            }
            else
            {
                this.DownloadClient = existingWebDownloadClient;
            }
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
            this.DownloadClient_BootWim.Start();
            this.DownloadClient_OSWim.Start();
        }

        public void Pause()
        {
            //Status = OSListRecordStatus.Active;
            Status = OSListRecordStatus.Paused;
            this.DownloadClient_BootWim.Pause();
            this.DownloadClient_OSWim.Pause();
        }
        #endregion
    }
}
