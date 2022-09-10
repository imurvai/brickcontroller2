using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class DeviceChannelLabel : Label
    {
        private readonly static string[] _controlPlusChannelLetters = new[] { "A", "B", "C", "D" };
        private readonly static string[] _circuitCubesChannelLetters = new[] { "A", "B", "C" };
        private readonly static string[] _buwizz3ChannelLetters = new[] { "1", "2", "3", "4", "A", "B" };

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
                case DeviceType.WeDo2:
                    Text = _controlPlusChannelLetters[Math.Min(Math.Max(Channel, 0), 3)];
                    break;

                case DeviceType.CircuitCubes:
                    Text = _circuitCubesChannelLetters[Math.Min(Math.Max(Channel, 0), 2)];
                    break;

                case DeviceType.BuWizz3:
                    Text = _buwizz3ChannelLetters[Math.Min(Math.Max(Channel, 0), 6)];
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
