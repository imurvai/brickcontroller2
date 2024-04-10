using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class TextToCapitalInitialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.TrimStart().Substring(0, 1).ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
