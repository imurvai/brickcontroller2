using System.Windows.Input;

namespace BrickController2.UI.Controls
{
    public class ExtendedSlider : Slider
    {
        public static BindableProperty TouchDownCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);
        public static BindableProperty TouchUpCommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null);
        public static BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(double), typeof(ExtendedSlider), 1.0, propertyChanged: OnStepChanged);

        public ExtendedSlider()
        {
            // due to (probably) Maui chnages, we need earlier event than ValueChanged
            PropertyChanging += ExtendedSlider_PropertyChanging;
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

        private static void OnStepChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ExtendedSlider slider && newValue is double newStep && newStep > 0)
            {
                slider.Value = Round(slider.Value, newStep);
            }
        }

        private void ExtendedSlider_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != ValueProperty.PropertyName || Step == 0)
                return;

            var newValue = Round(Value, Step);
            if (Value != newValue)
            {
                Value = newValue;
            }
        }

        private static double Round(double value, double step)
        {
            return Math.Round(value / step) * step;
        }
    }
}
