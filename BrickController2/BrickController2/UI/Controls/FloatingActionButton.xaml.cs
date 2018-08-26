using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FloatingActionButton : ContentView
	{
		public FloatingActionButton()
		{
			InitializeComponent();
		}

        public static BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(FloatingActionButton), default(Color), BindingMode.OneWay, null, ButtonColorChanged);
        public static BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(FloatingActionButton), null, BindingMode.OneWay, null, ImageSourceChanged);
        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null, BindingMode.OneWay, null, CommandChanged);

        public Color ButtonColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static void ButtonColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var fab = bindable as FloatingActionButton;
            if (fab != null)
            {
                fab.Frame.BackgroundColor = (Color)newValue;
            }
        }

        private static void ImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var fab = bindable as FloatingActionButton;
            if (fab != null)
            {
                fab.Image.Source = (ImageSource)newValue;
            }
        }

        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var fab = bindable as FloatingActionButton;
            if (fab != null)
            {
                fab.TapGuestureRecognizer.Command = (ICommand)newValue;
            }
        }
    }
}