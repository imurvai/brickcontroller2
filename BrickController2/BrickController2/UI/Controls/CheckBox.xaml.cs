﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckBox : ContentView
    {
        public CheckBox()
        {
            InitializeComponent();
            UpdateView();
            TapRecognizer.Command = new Command(() => Checked = !Checked);
        }

        public static BindableProperty CheckedProperty = BindableProperty.Create(nameof(Checked), typeof(bool), typeof(CheckBox), false, BindingMode.TwoWay, null, CheckedChanged);

        public bool Checked
        {
            get => (bool)GetValue(CheckedProperty);
            set => SetValue(CheckedProperty, value);
        }

        private static void CheckedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CheckBox checkBox && newValue is bool isChecked)
            {
                checkBox.Checked = isChecked;
                checkBox.UpdateView();
            }
        }

        private void UpdateView()
        {
            UncheckedShape.IsVisible = !Checked;
            CheckedShape.IsVisible = Checked;
        }
    }
}