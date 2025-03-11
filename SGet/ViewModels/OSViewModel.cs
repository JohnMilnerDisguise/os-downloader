using SGet.Commands;
using SGet.Models;
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

namespace SGet.ViewModels
{
    public class OSViewModel : ICentralPanelBaseViewModel
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
                }
            }
        }

        //Command Properties
        public ICommand AddOSToLibraryCommand { get; set; }
        public ICommand RemoveOSFromLibraryCommand { get; set; }

        public OSViewModel()
        {
            OSList = OSListManager.Instance.OSList;

            AddOSToLibraryCommand      = new RelayCommand(AddOSToLibrary, CanAddOSToLibrary);
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
                selectedOSRecord.IsSelected = true;
                SelectedOSRecord = selectedOSRecord;
            }
        }


        //Command Implementations 
        private bool CanAddOSToLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating CanAddOSToLibrary");
            return true;
        }

        private void AddOSToLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating AddOSToLibrary");
            throw new NotImplementedException();
        }

        private bool CanRemoveOSFromLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating CanRemoveOSFromLibrary");
            return true;
        }

        private void RemoveOSFromLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating RemoveOSFromLibrary");
            throw new NotImplementedException();
        }
    }
}
