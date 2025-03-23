using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IronworksTranslator.UI
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            int valueInt = (int)value;
            int parameterInt = int.Parse(parameter.ToString());

            return valueInt == parameterInt ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 