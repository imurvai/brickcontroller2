using BrickController2.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SegmentedControl : ContentView
	{
        private readonly IList<Label> _labels = new List<Label>();

        public SegmentedControl ()
		{
			InitializeComponent ();
		}

        public static BindableProperty ItemsPropery = BindableProperty.Create(nameof(Items), typeof(IEnumerable<string>), typeof(SegmentedControl), null, BindingMode.OneWay, null, ItemsChanged);
        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SegmentedControl), 0, BindingMode.TwoWay, null, SelectedIndexChanged);
        public static BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SegmentedControl));

        public IEnumerable<string> Items
        {
            get => (IEnumerable<string>)GetValue(ItemsPropery);
            set => SetValue(ItemsPropery, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, Math.Max(0, Math.Min(value, (Items?.Count() ?? 0) - 1)));
        }

        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        private static void ItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SegmentedControl rbg)
            {
                rbg.Build();
            }
        }

        private static void SelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SegmentedControl rbg)
            {
                var index = (int)newValue;
                rbg.SetSelection(index);
            }
        }

        private void Build()
        {
            StackLayout.Children.Clear();
            _labels.Clear();

            if (Items == null || Items.Count() == 0)
            {
                return;
            }

            for (int index = 0; index < Items.Count(); index++)
            {
                var buttonText = Items.ElementAt(index);

                var frame = new Frame { BackgroundColor = Color.Transparent, HasShadow = false };
                frame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new SafeCommand<int>((i) => ItemTapped(i)), CommandParameter = index });
                var label = new Label { Text = buttonText };
                frame.Content = label;
                _labels.Add(label);

                StackLayout.Children.Add(frame);
            }

            SetSelection(SelectedIndex);
        }

        private void ItemTapped(int index)
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
            if (Items == null || Items.Count() == 0)
            {
                return;
            }

            selectedIndex = Math.Max(0, Math.Min(Items.Count() - 1, selectedIndex));

            for (int i = 0; i < _labels.Count; i++)
            {
                _labels[i].FontAttributes = i == selectedIndex ? FontAttributes.Bold : FontAttributes.None;
            }
        }
    }
}