using OSDownloader.Models;
using OSDownloader.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSDownloader.Views
{
    /// <summary>
    /// Interaction logic for OSView.xaml
    /// </summary>
    public partial class OSView : System.Windows.Controls.UserControl
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

        private void OSView_HandleLoaded(object sender, RoutedEventArgs e)
        {
            string[,] rowColoursByState = new string[,]
            {
                //bg         bg sel     fg         fg sel
                { "#FFDDDD", "#ff7e7e", "#979191", "#7a7373" },   //validation error
                { "#f9fafd", "#e6eaf7", "#888888", "#555555" },   //not in library (greyish)
                { "#f8f8dd", "#ececa3", "#8c8c1d", "#3a3a0c" },   //paused
                { "#ebf8dd", "#d2efb2", "#6caf24", "#4d7c1a" },   //active
                { "#E8F5E9", "#C8E6C9", "#1B5E20", "#FFFFFF" }    //Finished
            };

            /* deepseek suggested this 
            string[,] rowColoursByState = new string[,]
            {
                // Background (Not Selected), Background (Selected), Foreground (Not Selected), Foreground (Selected)
                { "#FFEBEE", "#FFCDD2", "#C62828", "#FFFFFF" },   // Validation Error (Red)
                { "#F5F5F5", "#E0E0E0", "#616161", "#212121" },   // Not in Library (Gray)
                { "#FFF3E0", "#FFE0B2", "#EF6C00", "#FFFFFF" },   // Paused (Orange)
                { "#E8F5E9", "#C8E6C9", "#2E7D32", "#FFFFFF" },   // Active (Green)
                { "#E3F2FD", "#BBDEFB", "#1565C0", "#FFFFFF" }    // Finished (Blue)
            };
            */

            List<string>[] statusStringsByState = new List<string>[]
            {
                new List<string>() { "NOT VALID", "Error" },
                new List<string>() { "Not In Library" },
                new List<string>() { "To Be Added", "Paused", "Queued", "Deleted" },
                new List<string>() { "Active", "Waiting", "Downloading", "Pausing", "Deleting" },
                new List<string>() { "Completed" }
            };

            Style existingStyle = (Style)FindResource("OSGridStyle");
            Style newStyle = new Style(typeof(DataGridRow), existingStyle);
            for ( int i=0; i < statusStringsByState.Length; i++ )
            {
                List<string> StatusStringArrayForState = statusStringsByState[i];
                foreach (string StatusString in StatusStringArrayForState)
                {
                    MultiDataTrigger mdt_normal = new MultiDataTrigger();
                    mdt_normal.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding("StatusString"), Value = StatusString });
                    mdt_normal.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding() { RelativeSource = new RelativeSource(RelativeSourceMode.Self), Path = new PropertyPath("IsSelected") }, Value = false });
                    mdt_normal.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.BackgroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom(rowColoursByState[i,0])) });
                    mdt_normal.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.ForegroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom(rowColoursByState[i,2])) });

                    MultiDataTrigger mdt_selected = new MultiDataTrigger();
                    mdt_selected.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding("StatusString"), Value = StatusString });
                    mdt_selected.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding() { RelativeSource = new RelativeSource(RelativeSourceMode.Self), Path = new PropertyPath("IsSelected") }, Value = true });
                    mdt_selected.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.BackgroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom(rowColoursByState[i,1])) });
                    mdt_selected.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.ForegroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom(rowColoursByState[i,3])) });

                    newStyle.Triggers.Add(mdt_normal);
                    newStyle.Triggers.Add(mdt_selected);
                    
                }
            }
            dataGrid_osList.RowStyle = newStyle;

            /*
            MultiDataTrigger mdt = new MultiDataTrigger();
            mdt.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding("StatusString"), Value = "To Be Added" });
            //mdt.Conditions.Add(new Condition() { Binding = new System.Windows.Data.Binding() { RelativeSource = new RelativeSource(RelativeSourceMode.Self), Path = new PropertyPath("IsSelected") }, Value = true });
            mdt.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.ForegroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000FF")) });
            mdt.Setters.Add(new Setter() { Property = System.Windows.Controls.Control.BackgroundProperty, Value = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000")) });
            Style existingStyle = (Style)FindResource("OSGridStyle");
            Style newStyle = new Style(typeof(DataGridRow), existingStyle);
            newStyle.Triggers.Add(mdt);
            dataGrid_osList.RowStyle = newStyle;
            */
        }
    }
}
