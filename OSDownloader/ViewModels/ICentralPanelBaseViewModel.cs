using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSDownloader.ViewModels
{
    public interface ICentralPanelBaseViewModel
    {
        // Properties
        ICommand AddOSToLibraryCommand { get; }
        ICommand RemoveOSFromLibraryCommand { get; }
    }
}
