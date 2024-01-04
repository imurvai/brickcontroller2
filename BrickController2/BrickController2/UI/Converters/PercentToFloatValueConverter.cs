using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class PercentToFloatValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = (int)value;
            return percent / 100F;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var floatValue = (float)value;
            return (int)(floatValue * 100);
        }
    }
}
