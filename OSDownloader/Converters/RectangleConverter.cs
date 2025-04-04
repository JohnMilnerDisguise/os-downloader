using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace OSDownloader.Converters
{
    public class RectangleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is double width &&
                values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return new Rect(0, 0, 300, 25); // Default size
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("RectConverter does not support ConvertBack");
        }
    }
}