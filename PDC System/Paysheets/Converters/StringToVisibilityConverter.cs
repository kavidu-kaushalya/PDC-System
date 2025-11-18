using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PDC_System.Paysheets.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;

            if (string.IsNullOrWhiteSpace(path))
                return Visibility.Collapsed;

            if (!System.IO.File.Exists(path))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
