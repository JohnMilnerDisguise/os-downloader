using OSDownloader.Models;
using OSDownloader.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSDownloader.Views
{
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : UserControl
    {
        private List<string> propertyValues;
        private List<PropertyModel> propertiesList;

        public DownloadsView()
        {
            InitializeComponent();

            //initialise property names for grid
            propertyValues = new List<string>();
            propertiesList = new List<PropertyModel>();
        }

        #region Control Event Handlers

        private void downloadsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WebDownloadClient selectedDownloadRecord = null;
            if (dataGrid_downloadsGrid.SelectedItems.Count > 0)
            {
                selectedDownloadRecord = (WebDownloadClient)dataGrid_downloadsGrid.SelectedItem;
            }

            DownloadsViewModel viewModel = (DownloadsViewModel)this.DataContext;
            viewModel?.HandleSelectedDownloadChanged(selectedDownloadRecord);
        }

        #endregion

    }
}
