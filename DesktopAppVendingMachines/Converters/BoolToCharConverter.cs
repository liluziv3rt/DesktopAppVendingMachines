using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DesktopAppVendingMachines.Converters
{
    public class BoolToCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showPassword && parameter is string passwordChar)
            {
                return showPassword ? '\0' : passwordChar[0];
            }
            return '*';
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}