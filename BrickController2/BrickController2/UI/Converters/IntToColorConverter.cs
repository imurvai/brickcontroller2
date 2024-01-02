using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        private static readonly Color[] AppColors = { Colors.Brown, Colors.DarkGreen, Colors.DarkSlateGray, Colors.DarkOrchid, Colors.DimGray, Colors.OliveDrab };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            return AppColors[intValue % AppColors.Length];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
