using System;
using System.Globalization;
using System.Windows.Data;

namespace PDC_System.Paysheets.Converters
{
    public class MonthShortNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string monthStr && !string.IsNullOrWhiteSpace(monthStr))
            {
                // Try parsing "March 2026" → DateTime → "Mar 2026"
                if (DateTime.TryParse(monthStr, out DateTime date))
                {
                    return date.ToString("MMM yyyy"); // "Mar 2026"
                }

                // Fallback: split "March 2026" manually
                var parts = monthStr.Split(' ');
                if (parts.Length == 2 && DateTime.TryParseExact(parts[0], "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate))
                {
                    return $"{monthDate:MMM} {parts[1]}"; // "Mar 2026"
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}