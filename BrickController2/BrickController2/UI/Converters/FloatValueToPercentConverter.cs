using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class FloatValueToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var floatValue = (float)value;
            var result = (int)(floatValue * 100);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = (double)value;
            var result = (float)(percent / 100);
            return result;
        }
    }
}
