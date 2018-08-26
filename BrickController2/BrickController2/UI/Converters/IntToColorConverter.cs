using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        private static readonly Color[] _colors = new Color[] { Color.Azure, Color.Bisque, Color.BlueViolet, Color.Gainsboro, Color.GreenYellow, Color.Lavender, Color.LightCoral, Color.LightGray };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            return _colors[intValue % _colors.Length];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
