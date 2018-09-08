using BrickController2.HardwareServices.GameController;
using BrickController2.Helpers;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class GameControllerEventTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventType = (GameControllerEventType)value;
            switch (eventType)
            {
                case GameControllerEventType.Button:
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.ic_buttons.png");

                case GameControllerEventType.Axis:
                    return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.ic_joystick.png");

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
