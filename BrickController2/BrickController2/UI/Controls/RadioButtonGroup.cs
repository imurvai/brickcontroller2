using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class RadioButtonGroup : StackLayout
    {
        public static BindableProperty ButtonTextsPropery = BindableProperty.Create(nameof(ButtonTexts), typeof(IEnumerable<string>), typeof(RadioButtonGroup), null, BindingMode.OneWay, null, ButtonTextsChanged);
        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(RadioButtonGroup), 0, BindingMode.TwoWay, null, SelectedIndexChanged);
        public static BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(RadioButtonGroup));

        public IEnumerable<string> ButtonTexts
        {
            get => (IEnumerable<string>)GetValue(ButtonTextsPropery);
            set => SetValue(ButtonTextsPropery, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        private static void ButtonTextsChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        private static void SelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }
    }
}
