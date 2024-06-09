using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Controls
{
    public class ExtendedSlider : Slider
    {
        public static BindableProperty TouchDownCommandProperty = BindableProperty.Create(nameof(TouchDownCommand), typeof(ICommand), typeof(ExtendedSlider), null);
        public static BindableProperty TouchUpCommandProperty = BindableProperty.Create(nameof(TouchUpCommand), typeof(ICommand), typeof(ExtendedSlider), null);
        public static BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(double), typeof(ExtendedSlider), 1.0, propertyChanged: OnStepChanged);

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

        public void SetAndRoundNewValue(double value)
        {
            Value = Round(value, Step);
        }

        private static void OnStepChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ExtendedSlider slider && newValue is double newStep && newStep > 0)
            {
                slider.Value = Round(slider.Value, newStep);
            }
        }

        private static double Round(double value, double step)
        {
            return Math.Round(value / step) * step;
        }
    }
}
