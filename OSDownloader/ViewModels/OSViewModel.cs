using OSDownloader.Commands;
using OSDownloader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OSDownloader.ViewModels
{
    public class OSViewModel : ICentralPanelBaseViewModel, INotifyPropertyChanged
    {
        //ViewModel Event handlers
        public event PropertyChangedEventHandler PropertyChanged;

        //State Properties
        public ObservableCollection<OSListEntry> OSList { get; set; }

        private OSListEntry _selectedOSRecord;
        public OSListEntry SelectedOSRecord { 
            get {
                return _selectedOSRecord;
            } 
            set {
                if (value != _selectedOSRecord)
                {
                    _selectedOSRecord = value;
                    RaisePropertyChanged(nameof(SelectedOSRecord));
                    ((RelayCommand)AddOSToLibraryCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RemoveOSFromLibraryCommand).RaiseCanExecuteChanged();
                }
            }
        }

        //Command Properties
        public ICommand AddOSToLibraryCommand { get; set; }
        public ICommand RemoveOSFromLibraryCommand { get; set; }

        public OSViewModel()
        {
            OSList = OSListManager.Instance.OSList;
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

        //Control Event Handlers
        public void HandleSelectedOSChanged(OSListEntry selectedOSRecord)
        {
            foreach (OSListEntry osRecord in OSList)
            {
                osRecord.IsSelected = false;
            }
            if(selectedOSRecord != null)
            {
                SelectedOSRecord = selectedOSRecord;
                selectedOSRecord.IsSelected = true;
            }
        }


        //Command Implementations 
        private bool CanAddOSToLibrary(object obj)
        {
            return _selectedOSRecord != null && _selectedOSRecord.AllowUserToAddToLibrary;
        }

        private void AddOSToLibrary(object obj)
        {
            _selectedOSRecord.SelectedActionString = OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Add_To_Library);
        }

        private bool CanRemoveOSFromLibrary(object obj)
        {
            return _selectedOSRecord != null && _selectedOSRecord.AllowUserToRemoveFromLibrary;
        }

        private void RemoveOSFromLibrary(object obj)
        {
            _selectedOSRecord.SelectedActionString = OSListRecordEnumUtils.getActionDescriptionFromActionEnum(OSListRecordAction.Do_Not_Add);
        }


    }
}
