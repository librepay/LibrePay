using System;
using System.Globalization;
using Xamarin.Forms;

namespace BitcoinPOS_App.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => InvertBool(value, targetType);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => InvertBool(value, targetType);

        private static object InvertBool(object value, Type targetType)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool) value;
        }
    }
}