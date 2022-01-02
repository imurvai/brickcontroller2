using BrickController2.DeviceManagement;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToChannelOutputVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DeviceType deviceType)
            {
                switch (deviceType)
                {
                    case DeviceType.Boost:
                    case DeviceType.PoweredUp:
                    case DeviceType.TechnicHub:
                        return true;
                    case DeviceType.BuWizz3:
                        // visible only for PU ports
                        return values[1] is int channel && channel < BuWizz3Device.NUMBER_OF_PU_PORTS;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
