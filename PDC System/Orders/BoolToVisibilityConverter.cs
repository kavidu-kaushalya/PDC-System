using System;
using PDC_System.Orders;

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PDC_System.Orders
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFinished = (bool)value;
            bool invert = parameter as string == "Invert";

            if (invert)
                isFinished = !isFinished;

            return isFinished ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}