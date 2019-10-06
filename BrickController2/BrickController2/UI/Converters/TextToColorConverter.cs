using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class TextToColorConverter : IValueConverter
    {
        /// <summary>
        /// Get all known Xamarin colors
        /// </summary>
        private static readonly Dictionary<string, Color> KnownColors =
            typeof(Color)
            .GetProperties(BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(p => p.Name, p => (Color)p.GetValue(null));

        /// <summary>
        /// Some list of different colors
        /// </summary>
        private static readonly Color[] Colors =
        {
            Color.LightGreen,
            Color.MediumPurple,
            Color.Orange,
            Color.DeepSkyBlue,
            Color.IndianRed,
            Color.DeepPink,
            Color.AliceBlue
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Color.DimGray;
            }

            if (KnownColors.TryGetValue(text, out var color))
            {
                return color;
            }

            // get color based on the first letter
            return Colors[text[0] % Colors.Length];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
