using System;
using System.Globalization;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value;
            return Convert(deviceType);
        }

        public ImageSource Convert(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                    return ResourceHelper.GetImageResource("buwizz_image.png");

                case DeviceType.BuWizz3:
                    return ResourceHelper.GetImageResource("buwizz3_image.png");

                case DeviceType.SBrick:
                    return ResourceHelper.GetImageResource("sbrick_image.png");

                case DeviceType.Infrared:
                    return ResourceHelper.GetImageResource("infra_image.png");

                case DeviceType.PoweredUp:
                    return ResourceHelper.GetImageResource("poweredup_image.png");

                case DeviceType.Boost:
                    return ResourceHelper.GetImageResource("boost_image.png");

                case DeviceType.TechnicHub:
                    return ResourceHelper.GetImageResource("technichub_image.png");

                case DeviceType.DuploTrainHub:
                    return ResourceHelper.GetImageResource("duplotrainhub_image.png");

                case DeviceType.CircuitCubes:
                    return ResourceHelper.GetImageResource("circuitcubes_image.png");

                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
