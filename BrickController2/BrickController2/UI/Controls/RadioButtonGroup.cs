using BrickController2.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class RadioButtonGroup : StackLayout
    {
        private readonly IList<Label> _labels = new List<Label>();

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
            if (bindable is RadioButtonGroup rbg)
            {
                rbg.Build();
            }
        }

        private static void SelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RadioButtonGroup rbg)
            {
                var index = (int)newValue;
                rbg.SetSelection(index);
            }
        }

        private void Build()
        {
            Children.Clear();
            _labels.Clear();

            if (ButtonTexts == null || ButtonTexts.Count() == 0)
            {
                return;
            }

            int index = 0;
            foreach (var buttonText in ButtonTexts)
            {
                var frame = new Frame { BackgroundColor = Color.Transparent, HasShadow = false };
                frame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new SafeCommand<int>(async (i) => await ButtonTapped(i)), CommandParameter = index });
                var label = new Label { Text = buttonText };
                frame.Content = label;
                _labels.Add(label);

                Children.Add(frame);
            }

            SetSelection(SelectedIndex);
        }

        private async Task ButtonTapped(int index)
        {
            if (SelectedIndex == index)
            {
                return;
            }

            if (SelectionChangedCommand != null && SelectionChangedCommand.CanExecute(index))
            {
                SelectionChangedCommand.Execute(index);
            }

            SetSelection(index);
        }

        private void SetSelection(int selectedIndex)
        {
            if (ButtonTexts == null || ButtonTexts.Count() == 0)
            {
                return;
            }

            selectedIndex = Math.Max(0, Math.Min(ButtonTexts.Count() - 1, selectedIndex));

            for(int i = 0; i < _labels.Count; i++)
            {
                _labels[i].FontAttributes = i == selectedIndex ? FontAttributes.Bold : FontAttributes.None;
            }
        }
    }
}
