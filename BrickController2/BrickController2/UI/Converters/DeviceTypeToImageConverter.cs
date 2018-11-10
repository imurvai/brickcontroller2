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
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.buwizz_image.png");

                case DeviceType.SBrick:
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.sbrick_image.png");

                case DeviceType.Infrared:
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.infra_image.png");

                case DeviceType.PoweredUp:
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.poweredup_image.png");

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
