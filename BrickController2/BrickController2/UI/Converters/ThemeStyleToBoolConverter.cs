using BrickController2.UI.Services.Theme;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class ThemeStyleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThemeType theme1 && parameter is ThemeType theme2)
            {
                return theme1 == theme2;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
