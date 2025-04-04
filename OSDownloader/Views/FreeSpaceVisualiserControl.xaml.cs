using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OSDownloader.Views
{
    public partial class FreeSpaceVisualiserControl : UserControl
    {
        #region Dependency Properties

        // Space properties
        public static readonly DependencyProperty TotalSpaceProperty =
            DependencyProperty.Register("TotalSpace", typeof(double), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(1000.0, OnSpaceValuesChanged));

        public static readonly DependencyProperty UsedSpaceProperty =
            DependencyProperty.Register("UsedSpace", typeof(double), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(0.0, OnSpaceValuesChanged));

        public static readonly DependencyProperty RequiredSpaceProperty =
            DependencyProperty.Register("RequiredSpace", typeof(double), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(0.0, OnSpaceValuesChanged));

        // Color properties
        public static readonly DependencyProperty IsDisabledFillProperty =
            DependencyProperty.Register("IsDisabledFill", typeof(Brush), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44))));

        public static readonly DependencyProperty UsedSpaceFillProperty =
            DependencyProperty.Register("UsedSpaceFill", typeof(Brush), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0x57, 0x22))));

        public static readonly DependencyProperty RequiredSpaceFillProperty =
            DependencyProperty.Register("RequiredSpaceFill", typeof(Brush), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07))));

        public static readonly DependencyProperty FreeSpaceFillProperty =
            DependencyProperty.Register("FreeSpaceFill", typeof(Brush), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x8B, 0xC3, 0x4A))));

        // Size properties
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(FreeSpaceVisualiserControl),
                new PropertyMetadata(12.5, OnSpaceValuesChanged));

        #endregion

        #region Property Accessors

        public double TotalSpace
        {
            get => (double)GetValue(TotalSpaceProperty);
            set => SetValue(TotalSpaceProperty, value);
        }

        public double UsedSpace
        {
            get => (double)GetValue(UsedSpaceProperty);
            set => SetValue(UsedSpaceProperty, value);
        }

        public double RequiredSpace
        {
            get => (double)GetValue(RequiredSpaceProperty);
            set => SetValue(RequiredSpaceProperty, value);
        }

        public Brush IsDisabledFill
        {
            get => (Brush)GetValue(IsDisabledFillProperty);
            set => SetValue(IsDisabledFillProperty, value);
        }

        public Brush UsedSpaceFill
        {
            get => (Brush)GetValue(UsedSpaceFillProperty);
            set => SetValue(UsedSpaceFillProperty, value);
        }

        public Brush RequiredSpaceFill
        {
            get => (Brush)GetValue(RequiredSpaceFillProperty);
            set => SetValue(RequiredSpaceFillProperty, value);
        }

        public Brush FreeSpaceFill
        {
            get => (Brush)GetValue(FreeSpaceFillProperty);
            set => SetValue(FreeSpaceFillProperty, value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public FreeSpaceVisualiserControl()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            UpdateVisualization();
        }

        private static void OnSpaceValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FreeSpaceVisualiserControl;
            control?.UpdateVisualization();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualization();
        }

        private void UpdateVisualization()
        {
            if (TotalSpace <= 0 || ActualWidth <= 0 || ActualHeight <= 0)
            {
                labelFreeSpaceOverlay.Content = "Not in use";
                controlIsDisabledRect.Visibility = Visibility.Visible;
                return;
            }

            double totalWidth = ActualWidth;
            double usedWidth = (UsedSpace / TotalSpace) * totalWidth;
            double requiredWidth = (RequiredSpace / TotalSpace) * totalWidth;
            double freeWidth = Math.Max(0, totalWidth - usedWidth - requiredWidth);

            // Set widths
            UsedSpaceRect.Width = usedWidth;
            RequiredSpaceRect.Width = requiredWidth;
            FreeSpaceRect.Width = freeWidth;

            // Position the rectangles
            Canvas.SetLeft(UsedSpaceRect, 0);
            Canvas.SetLeft(RequiredSpaceRect, usedWidth);
            Canvas.SetLeft(FreeSpaceRect, usedWidth + requiredWidth);

            // Update corner radius (half of height)
            CornerRadius = ActualHeight / 2;

            //update free space text overlay
            double sizeInGB = TotalSpace / (1024.0 * 1024.0 * 1024.0); // Convert bytes to GB
            double remainingInGB = ( TotalSpace - UsedSpace - RequiredSpace ) / (1024.0 * 1024.0 * 1024.0);
            if(remainingInGB < 0)
            {
                remainingInGB = 0.0;
            }
            controlIsDisabledRect.Visibility = Visibility.Hidden;
            labelFreeSpaceOverlay.Content = $"{Math.Round(remainingInGB, 1):0.0} of {Math.Round(sizeInGB, 1):0.0} GB free"; // Round to 1 decimal place and add GB for the units

        }
    }
}