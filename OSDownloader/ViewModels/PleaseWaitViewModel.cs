using OSDownloader.Commands;
using OSDownloader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OSDownloader.ViewModels
{
    public class PleaseWaitViewModel : ICentralPanelBaseViewModel
    {
        //Command Properties
        public ICommand AddOSToLibraryCommand { get; set; }
        public ICommand RemoveOSFromLibraryCommand { get; set; }

        public PleaseWaitViewModel()
        {
            AddOSToLibraryCommand      = new RelayCommand(AddOSToLibrary, CanAddOSToLibrary);
            RemoveOSFromLibraryCommand = new RelayCommand(RemoveOSFromLibrary, CanRemoveOSFromLibrary);
        }

        private bool CanAddOSToLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating CanAddOSToLibrary");
            return false;
        }

        private void AddOSToLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating AddOSToLibrary");
            throw new NotImplementedException();
        }

        private bool CanRemoveOSFromLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating CanRemoveOSFromLibrary");
            return false;
        }

        private void RemoveOSFromLibrary(object obj)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating RemoveOSFromLibrary");
            throw new NotImplementedException();
        }
    }
}
