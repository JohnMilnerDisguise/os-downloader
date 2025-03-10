using SGet.Commands;
using SGet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SGet.ViewModels
{
    public class OSViewModel
    {
        //State Properties
        public ObservableCollection<OSListEntry> OSList { get; set; }

        //Command Properties
        public ICommand SetOSInclisionInLibraryCommand { get; set; }

        public OSViewModel()
        {
            OSList = OSListManager.Instance.OSList;

            SetOSInclisionInLibraryCommand = new RelayCommand(SetOSInclisionInLibrary, CanSetOSInclisionInLibrary);
            //OSList.Add(new OSListEntry("uid", new List<string> { "ZX Spectrum" }, "johnnyOS", "http://google.com", "johnnyOS.wim",
            //    1234567, "http://google.com", "boot_1234.wim", 12345, "This is the best OS in the world man", null, null));
            //OSListManager.Instance.OSList.CollectionChanged += new NotifyCollectionChangedEventHandler(OSList_CollectionChanged);
        }

        private bool CanSetOSInclisionInLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating CanSetOSInclisionInLibrary");
            return true;
        }

        private void SetOSInclisionInLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating SetOSInclisionInLibrary");
            throw new NotImplementedException();
        }
    }
}
