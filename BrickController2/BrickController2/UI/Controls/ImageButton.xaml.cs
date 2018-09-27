using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageButton), null, BindingMode.OneWay, null, CommandChanged);
        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(Command), typeof(object), typeof(ImageButton), null, BindingMode.OneWay, null, CommandParameterChanged);

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
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
            if (bindable is ImageButton imageButton)
            {
                imageButton.Image.Source = (ImageSource)newValue;
            }
        }

        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton)
            {
                imageButton.TapGuestureRecognizer.Command = (ICommand)newValue;
            }
        }

        private static void CommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton)
            {
                imageButton.TapGuestureRecognizer.CommandParameter = newValue;
            }
        }
    }
}