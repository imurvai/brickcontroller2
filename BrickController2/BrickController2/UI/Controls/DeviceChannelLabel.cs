﻿using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class DeviceChannelLabel : Label
    {
        private readonly static string[] _controlPlusChannelLetters = new[] { "A", "B", "C", "D" };

        public static BindableProperty DeviceTypeProperty = BindableProperty.Create(nameof(DeviceType), typeof(DeviceType), typeof(DeviceChannelLabel), default(DeviceType), BindingMode.OneWay, null, OnDeviceChanged);
        public static BindableProperty ChannelProperty = BindableProperty.Create(nameof(Channel), typeof(int), typeof(DeviceChannelLabel), 0, BindingMode.OneWay, null, OnChannelChanged);

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

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelLabel dcl)
            {
                dcl.SetChannelText();
            }
        }

        private static void OnChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelLabel dcl)
            {
                dcl.SetChannelText();
            }
        }

        private void SetChannelText()
        {
            switch (DeviceType)
            {
                case DeviceType.Boost:
                case DeviceType.DuploTrainHub:
                case DeviceType.PoweredUp:
                case DeviceType.TechnicHub:
                    Text = _controlPlusChannelLetters[Math.Min(Math.Max(Channel, 0), 3)];
                    break;

                case DeviceType.Infrared:
                    Text = Channel == 0 ?
                        TranslationHelper.Translate("Blue") :
                        TranslationHelper.Translate("Red");
                    break;

                default:
                    Text = $"{Channel + 1}";
                    break;
            }
        }
    }
}
