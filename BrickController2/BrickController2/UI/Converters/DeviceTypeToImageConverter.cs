using System;
using System.Globalization;
using BrickController2.DeviceManagement;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value;
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                    return ImageSource.FromResource("BrickController2.UI.Images.buwizz_image.png");

                case DeviceType.SBrick:
                    return ImageSource.FromResource("BrickController2.UI.Images.sbrick_image.png");

                case DeviceType.Infrared:
                    return ImageSource.FromResource("BrickController2.UI.Images.infra_image.png");

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
