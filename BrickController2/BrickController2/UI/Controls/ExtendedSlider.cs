using System.Windows.Input;

namespace BrickController2.UI.Controls
{
    public class ExtendedSlider : Slider
    {
        private const double DefaultStepValue = 1;

        public static readonly BindableProperty TouchDownCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);
        public static readonly BindableProperty TouchUpCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);
        public static readonly BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(double), typeof(ExtendedSlider), DefaultStepValue, coerceValue: CoerceStepValue);

        public ExtendedSlider()
        {
            // appliceation of step is done in handlers (or nativly) per each platform
        }

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

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
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

        public void SetValue(double newValue)
        {
            // update value based on current step value
            Value = Round(newValue, Step);
        }

        private static object CoerceStepValue(object bindable, object value)
        {
            return Math.Max((double)value, DefaultStepValue);
        }

        private static double Round(double value, double step)
        {
            return Math.Round(value / step) * step;
        }
    }
}
