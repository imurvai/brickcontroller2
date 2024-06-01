using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using BrickController2.UI.Commands;

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

        public static BindableProperty ItemsCsvPropery = BindableProperty.Create(nameof(ItemsCsv), typeof(string), typeof(SegmentedControl), default(string), propertyChanged: ItemsCsvChanged);
        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SegmentedControl), -1, BindingMode.TwoWay, propertyChanged: SelectedIndexChanged, coerceValue: CoerceSelectedIndex);
        public static BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SegmentedControl));

        public string ItemsCsv
        {
            get => (string)GetValue(ItemsCsvPropery);
            set => SetValue(ItemsCsvPropery, value);
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

        private static object CoerceSelectedIndex(BindableObject bindable, object newValue)
        {
            var segmentedControl = (SegmentedControl)bindable;
            var intValue = (int)newValue;
            return Math.Max(-1, Math.Min(segmentedControl._labels.Count - 1, intValue));
        }

        private static void ItemsCsvChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SegmentedControl segmentedControl && newValue is string itemsCsv)
            {
                var items = itemsCsv?.Split(',').Select(i => i.Trim()).ToList() ?? new List<string>();
                segmentedControl.Build(items);
            }

        }

        private static void SelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SegmentedControl segmentedControl && newValue is int index)
            {
                segmentedControl.SetSelection(index);
            }
        }

        private void Build(IList<string> items)
        {
            MyStackLayout.Children.Clear();
            _labels.Clear();

            if (items == null || items.Count() == 0)
            {
                return;
            }

            for (int index = 0; index < items.Count(); index++)
            {
                var buttonText = items.ElementAt(index);

                if (index != 0)
                {
                    var separator = new BoxView
                    {
                        WidthRequest = 1,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = new Thickness(1)
                    };
                    separator.SetDynamicResource(BoxView.BackgroundColorProperty, "DividerColor");

                    MyStackLayout.Children.Add(separator);
                }

#pragma warning disable CS0618 // Type or member is obsolete
                var label = new Label
                {
                    Text = buttonText,
                    FontAttributes = FontAttributes.None,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center
                };
#pragma warning restore CS0618 // Type or member is obsolete
                _labels.Add(label);

                var frame = new Frame
                {
                    BackgroundColor = Colors.Transparent,
                    Padding = new Thickness(1),
                    HasShadow = false
                };
                frame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new SafeCommand<int>(i => ItemTapped(i)), CommandParameter = index });
                frame.Content = label;

                MyStackLayout.Children.Add(frame);
            }

            SetSelection(SelectedIndex);
        }

        private void ItemTapped(int index)
        {
            if (SelectedIndex == index)
            {
                return;
            }

            SelectedIndex = index;

            if (SelectionChangedCommand != null && SelectionChangedCommand.CanExecute(index))
            {
                SelectionChangedCommand.Execute(index);
            }
        }

        private void SetSelection(int selectedIndex)
        {
            if (_labels.Count == 0)
            {
                return;
            }

            selectedIndex = Math.Max(-1, Math.Min(_labels.Count() - 1, selectedIndex));

            for (int i = 0; i < _labels.Count; i++)
            {
                _labels[i].SetDynamicResource(Label.TextColorProperty, i == selectedIndex ? "PrimaryTextColor" : "SecondaryTextColor");
                _labels[i].FontAttributes = i == selectedIndex ? FontAttributes.Bold : FontAttributes.None;
            }
        }
    }
}