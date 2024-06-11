using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImageButton : ContentView
	{
		public ImageButton ()
		{
			InitializeComponent ();
		}

        public static BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ImageButton), null, BindingMode.OneWay, null, ImageSourceChanged);
        public static BindableProperty ImageColorProperty = BindableProperty.Create(nameof(ImageColor), typeof(Color), typeof(ImageButton), default(Color), BindingMode.OneWay, null, ImageColorChanged);
        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageButton), null, BindingMode.OneWay, null, CommandChanged);
        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(Command), typeof(object), typeof(ImageButton), null, BindingMode.OneWay, null, CommandParameterChanged);

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public Color ImageColor
        {
            get => (Color)GetValue(ImageColorProperty);
            set => SetValue(ImageColorProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private static void ImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is ImageSource imageSource)
            {
                imageButton.MyImage.Source = imageSource;
            }
        }

        private static void ImageColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is Color color)
            {
                imageButton.MyImage.Color = color;
            }
        }

        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is ICommand command)
            {
                imageButton.MyTapGuestureRecognizer.Command = command;
            }
        }

        private static void CommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton)
            {
                imageButton.MyTapGuestureRecognizer.CommandParameter = newValue;
            }
        }
    }
}