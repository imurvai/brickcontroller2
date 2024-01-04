using BrickController2.PlatformServices.GameController;
using BrickController2.Helpers;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class GameControllerEventTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventType = (GameControllerEventType)value;
            return Convert(eventType);
        }

        public ImageSource Convert(GameControllerEventType eventType)
        {
            switch (eventType)
            {
                case GameControllerEventType.Button:
                    return ResourceHelper.GetImageResource("ic_buttons.png");

                case GameControllerEventType.Axis:
                    return ResourceHelper.GetImageResource("ic_joystick.png");

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
