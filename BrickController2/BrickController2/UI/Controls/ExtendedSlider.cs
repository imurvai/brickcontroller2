using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class ExtendedSlider : Slider
    {
        public static BindableProperty TouchDownCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);
        public static BindableProperty TouchUpCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);

        public ICommand TouchDownCommand
        {
            get => (ICommand)GetValue(TouchDownCommandProperty);
            set => SetValue(TouchDownCommandProperty, value);
        }

        public ICommand TouchUpCommand
        {
            get => (ICommand)GetValue(TouchUpCommandProperty);
            set => SetValue(TouchUpCommandProperty, value);
        }

        public void TouchDown()
        {
            if (TouchDownCommand != null && TouchDownCommand.CanExecute(null))
            {
                TouchDownCommand.Execute(null);
            }
        }

        public void TouchUp()
        {
            if (TouchUpCommand != null && TouchUpCommand.CanExecute(null))
            {
                TouchUpCommand.Execute(null);
            }
        }
    }
}
