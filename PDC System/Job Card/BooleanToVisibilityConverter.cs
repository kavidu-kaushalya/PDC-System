using System;
using System.Windows;
using System.Windows.Data;

namespace PDC_System
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always return Visibility.Visible
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert Visibility back to bool, this part might be unused in your case.
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
