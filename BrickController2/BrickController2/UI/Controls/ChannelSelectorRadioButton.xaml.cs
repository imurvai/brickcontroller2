﻿using BrickController2.DeviceManagement;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelSelectorRadioButton : ContentView
    {
        public ChannelSelectorRadioButton()
        {
            InitializeComponent();
        }

        public static BindableProperty DeviceTypeProperty = BindableProperty.Create(nameof(DeviceType), typeof(DeviceType), typeof(ChannelSelectorRadioButton), default(DeviceType), BindingMode.OneWay, null, OnDeviceTypeChanged);
        public static BindableProperty ChannelProperty = BindableProperty.Create(nameof(Channel), typeof(int), typeof(ChannelSelectorRadioButton), 0, BindingMode.OneWay, null, OnChannelChanged);
        public static BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(ChannelSelectorRadioButton), 0, BindingMode.OneWay, null, OnSelectedChannelChanged);
        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChannelSelectorRadioButton), null, BindingMode.OneWay, null, OnCommandChanged);

        public DeviceType DeviceType
        {
            get => (DeviceType)GetValue(DeviceTypeProperty);
            set => SetValue(DeviceTypeProperty, value);
        }

        public int Channel
        {
            get => (int)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        public int SelectedChannel
        {
            get => (int)GetValue(SelectedChannelProperty);
            set => SetValue(SelectedChannelProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        private static void OnDeviceTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.ChannelLabel.DeviceType = (DeviceType)newValue;
            }
        }

        private static void OnChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                int channel = (int)newValue;
                csrb.CheckedIndicatorFrame.IsVisible = csrb.SelectedChannel == channel;
                csrb.ChannelLabel.Channel = channel;
            }
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.CheckedIndicatorFrame.IsVisible = csrb.Channel == (int)newValue;
            }
        }

        private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.TapRecognizer.Command = (ICommand)newValue;
            }
        }
    }
}