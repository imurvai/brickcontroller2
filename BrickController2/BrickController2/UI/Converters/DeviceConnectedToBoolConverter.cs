using BrickController2.DeviceManagement;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class DeviceConnectedToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var deviceState = (DeviceState)value;
            return deviceState == DeviceState.Connected;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
