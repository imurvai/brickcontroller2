using System.Globalization;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Converters
{
    internal class DeviceAndChannelToMaxServoAngleVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Device device)
            {
                return values[1] is int channel && device.CanChangeMaxServoAngle(channel);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}