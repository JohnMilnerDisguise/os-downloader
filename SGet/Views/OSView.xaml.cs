using SGet.Models;
using SGet.ViewModels;
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

namespace SGet.Views
{
    /// <summary>
    /// Interaction logic for OSView.xaml
    /// </summary>
    public partial class OSView : UserControl
    {

        public OSView()
        {
            InitializeComponent();
        }



        #region Control Event Handlers

        private void dataGrid_osList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OSListEntry selectedOSRecord = null;
            if (dataGrid_osList.SelectedItems.Count > 0)
            {
                selectedOSRecord = (OSListEntry)dataGrid_osList.SelectedItem;
            }

            OSViewModel viewModel = (OSViewModel)this.DataContext;
            viewModel?.HandleSelectedOSChanged(selectedOSRecord);
        }

        #endregion

        private void releaseNotesPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Evaluating releaseNotesPanel_DataContextChanged");
        }
    }
}
