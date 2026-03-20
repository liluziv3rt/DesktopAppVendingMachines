using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DesktopAppVendingMachines.Converters
{
    public class BoolToEyeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showPassword)
            {
                return showPassword ? "🙈" : "👁️";
            }
            return "👁️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}