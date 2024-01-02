namespace BrickController2.UI.Controls
{
    // Reference: https://github.com/shrutinambiar/xamarin-forms-tinted-image
    public class ColorImage : Image
    {
        public static BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(ColorImage), default(Color));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
    }
}
