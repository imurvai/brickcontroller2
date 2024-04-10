using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class ChannelToDisplayChannelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channel = (int)value;
            return $"{channel + 1}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
