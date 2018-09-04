using System;
using System.Globalization;
using BrickController2.DeviceManagement;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        private const string ImagesNamespacePrefix = "BrickController2.UI.Images.";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value;
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                    return ImageSource.FromResource($"{ImagesNamespacePrefix}buwizz_image.png");

                case DeviceType.SBrick:
                    return ImageSource.FromResource($"{ImagesNamespacePrefix}sbrick_image.png");

                case DeviceType.Infrared:
                    return ImageSource.FromResource($"{ImagesNamespacePrefix}infra_image.png");

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
