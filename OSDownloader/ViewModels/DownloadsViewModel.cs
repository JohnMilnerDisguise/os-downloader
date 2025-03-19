using OSDownloader.Commands;
using OSDownloader.Models;
using OSDownloader.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSDownloader.ViewModels
{
    public class DownloadsViewModel : ICentralPanelBaseViewModel, INotifyPropertyChanged
    {
        //ViewModel Event handlers
        public event PropertyChangedEventHandler PropertyChanged;

        //State Properties
        public ObservableCollection<WebDownloadClient> DownloadsList { get; set; }

        private WebDownloadClient _selectedDownloadRecord;
        public WebDownloadClient SelectedDownloadRecord
        {
            get
            {
                return _selectedDownloadRecord;
            }
            set
            {
                if (value != _selectedDownloadRecord)
                {
                    _selectedDownloadRecord = value;
                    SelectedDownloadPropertiesList = DownloadManager.GetDownloadClientsPropertiesList(_selectedDownloadRecord);
                    RaisePropertyChanged(nameof(SelectedDownloadRecord));
                    //dont need to invalidate the Can*SelectedDownload* Commands as these get re-evaluated every time the context menu gets rendered anyway
                    //((RelayCommand)StartSelectedDownloadCommand).RaiseCanExecuteChanged();
                    //((RelayCommand)PauseSelectedDownloadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private ObservableCollection<PropertyModel> _selectedDownloadPropertiesList;
        public ObservableCollection<PropertyModel> SelectedDownloadPropertiesList {
            get { return _selectedDownloadPropertiesList; }
            set
            {
                if (_selectedDownloadPropertiesList != value)
                {
                    _selectedDownloadPropertiesList = value;
                    RaisePropertyChanged(nameof(SelectedDownloadPropertiesList));
                }
            }
        }

        //Command Properties
        //Inherited from Main ViewModel
        public ICommand AddOSToLibraryCommand { get; set; }
        public ICommand RemoveOSFromLibraryCommand { get; set; }
        //Specific to This ViewModel
        public ICommand StartSelectedDownloadCommand { get; set; }
        public ICommand PauseSelectedDownloadCommand { get; set; }
        public ICommand OpenSelectedDownloadInWindowsFileExplorerCommand { get; set; }
        public ICommand OpenDownloadsFolderInWindowsFileExplorerCommand { get; set; }

        //Constructor
        public DownloadsViewModel()
        {
            DownloadsList = DownloadManager.Instance.DownloadsList;
            //SelectedOSRecord = new OSListEntry("uid", new List<string> { "ZX Spectrum" }, "johnnyOS", "http://google.com", "johnnyOS.wim",
            //                                   1234567, "http://google.com", "boot_1234.wim", 12345, "This is the best OS in the world man", null, null);

            AddOSToLibraryCommand = new RelayCommand(AddOSToLibrary, CanAddOSToLibrary);
            RemoveOSFromLibraryCommand = new RelayCommand(RemoveOSFromLibrary, CanRemoveOSFromLibrary);
            StartSelectedDownloadCommand = new RelayCommand(StartSelectedDownload, CanStartSelectedDownload);
            PauseSelectedDownloadCommand = new RelayCommand(PauseSelectedDownload, CanPauseSelectedDownload);
            OpenSelectedDownloadInWindowsFileExplorerCommand = new RelayCommand(OpenSelectedDownloadInWindowsFileExplorer, CanOpenSelectedDownloadInWindowsFileExplorer);
            OpenDownloadsFolderInWindowsFileExplorerCommand = new RelayCommand(OpenDownloadsFolderInWindowsFileExplorer, CanOpenDownloadsFolderInWindowsFileExplorer);

            //OSList.Add(new OSListEntry("uid", new List<string> { "ZX Spectrum" }, "johnnyOS", "http://google.com", "johnnyOS.wim",
            //    1234567, "http://google.com", "boot_1234.wim", 12345, "This is the best OS in the world man", null, null));
            //OSListManager.Instance.OSList.CollectionChanged += new NotifyCollectionChangedEventHandler(OSList_CollectionChanged);
        }

        //ViewModel Event Handlers
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void HandleSelectedDownloadChanged(WebDownloadClient selectedDownloadRecord)
        {
            SelectedDownloadRecord = selectedDownloadRecord;
        }

        #region Command Implementations 
        //Command Implementations Inhetited from Main ViewModel

        private bool CanAddOSToLibrary(object obj)
        {
            return false;
        }

        private void AddOSToLibrary(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanRemoveOSFromLibrary(object obj)
        {
            return false;
        }

        private void RemoveOSFromLibrary(object obj)
        {
            throw new NotImplementedException();
        }

        //Command Implementations Specific to This Control
        private bool CanStartSelectedDownload(object obj)
        {
            return _selectedDownloadRecord != null && 
                   _selectedDownloadRecord.Status != DownloadStatus.Completed;
        }

        private void StartSelectedDownload(object obj)
        {
            _selectedDownloadRecord.Start();
        }

        private bool CanPauseSelectedDownload(object obj)
        {
            return _selectedDownloadRecord != null && 
                   _selectedDownloadRecord.Status != DownloadStatus.Completed && 
                   _selectedDownloadRecord.Status != DownloadStatus.Pausing && 
                   _selectedDownloadRecord.Status != DownloadStatus.Paused;
        }

        private void PauseSelectedDownload(object obj)
        {
            _selectedDownloadRecord.Pause();
        }

        private bool CanOpenSelectedDownloadInWindowsFileExplorer(object obj)
        {
            return _selectedDownloadRecord != null &&
                   _selectedDownloadRecord.DownloadPath != null &&
                   _selectedDownloadRecord.DownloadedSize > 0;
        }

        private void OpenSelectedDownloadInWindowsFileExplorer(object obj)
        {
            if (_selectedDownloadRecord == null || _selectedDownloadRecord.DownloadPath == null )
            {
                return;
            }
            if( File.Exists(_selectedDownloadRecord.DownloadPath) ) {
                string argument = "/select, \"" + _selectedDownloadRecord.DownloadPath + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
            else if( File.Exists(_selectedDownloadRecord.TempDownloadPath) )
            {
                string argument = "/select, \"" + _selectedDownloadRecord.TempDownloadPath + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }

            //old implementation
            //var download = (WebDownloadClient)downloadsGrid.SelectedItem;
            //int lastIndex = _selectedDownloadRecord.DownloadPath.LastIndexOf("\\");
            //string directory = download.DownloadPath.Remove(lastIndex + 1);
            //if (Directory.Exists(directory))
            //{
            //    Process.Start(@directory);
            //}
        }

        private bool CanOpenDownloadsFolderInWindowsFileExplorer(object obj)
        {
            return true;
        }

        private void OpenDownloadsFolderInWindowsFileExplorer(object obj)
        {
            string downloadsFolder = Settings.Default.DownloadLocation;
            if (downloadsFolder == null || downloadsFolder.Trim().Length == 0 || !Directory.Exists(downloadsFolder))
            {
                return;
            }
            Process.Start(@downloadsFolder);
        }

        #endregion
    }
}
