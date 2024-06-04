using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        private static readonly Color[] ColorArray = { Colors.Brown, Colors.DarkGreen, Colors.DarkSlateGray, Colors.DarkOrchid, Colors.DimGray, Colors.OliveDrab };

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var intValue = (int)value!;
            return ColorArray[intValue % ColorArray.Length];
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
