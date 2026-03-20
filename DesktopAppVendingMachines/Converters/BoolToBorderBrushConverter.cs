using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace DesktopAppVendingMachines.Converters
{
    public class BoolToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid && isValid)
            {
                return new SolidColorBrush(Color.Parse("#27AE60")); // зеленый
            }
            return new SolidColorBrush(Color.Parse("#E74C3C")); // красный
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}