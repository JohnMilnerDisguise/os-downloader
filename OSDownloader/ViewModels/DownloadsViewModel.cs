using OSDownloader.Commands;
using OSDownloader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


        private ObservableCollection<PropertyModel> _selectedDownloadPropertiesList;
        public ObservableCollection<PropertyModel> SelectedDownloadPropertiesList {
            get { return _selectedDownloadPropertiesList; }
            set
            {
                if (_selectedDownloadPropertiesList != value)
                {
                    _selectedDownloadPropertiesList = value;
                    RaisePropertyChanged("SelectedDownloadPropertiesList");
                }
            }
        }

        //Command Properties
        public ICommand AddOSToLibraryCommand { get; set; }
        public ICommand RemoveOSFromLibraryCommand { get; set; }

        //Constructor
        public DownloadsViewModel()
        {
            DownloadsList = DownloadManager.Instance.DownloadsList;
            //SelectedOSRecord = new OSListEntry("uid", new List<string> { "ZX Spectrum" }, "johnnyOS", "http://google.com", "johnnyOS.wim",
            //                                   1234567, "http://google.com", "boot_1234.wim", 12345, "This is the best OS in the world man", null, null);

            AddOSToLibraryCommand = new RelayCommand(AddOSToLibrary, CanAddOSToLibrary);
            RemoveOSFromLibraryCommand = new RelayCommand(RemoveOSFromLibrary, CanRemoveOSFromLibrary);
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

        internal void HandleSelectedDownloadChanged(WebDownloadClient selectedOSRecord)
        {
            SelectedDownloadPropertiesList = DownloadManager.GetDownloadClientsPropertiesList(selectedOSRecord);
        }

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


    }
}
