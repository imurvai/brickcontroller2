using BrickController2.Helpers;
using BrickController2.UI.Converters;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeviceOutputTesterControl : ContentView
	{
		public DeviceOutputTesterControl ()
		{
			InitializeComponent ();
		}

        public static BindableProperty DeviceProperty = BindableProperty.Create(nameof(Device), typeof(Device), typeof(DeviceOutputTesterControl), propertyChanged: OnDeviceChanged);

        public Device Device
        {
            get => (Device)GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceOutputTesterControl dotc && newValue is Device device)
            {
                dotc.Setup(device);
            }
        }

        private void Setup(Device device)
        {
            StackLayout.Clear();

            for (int channel = 0; channel < device.NumberOfChannels; channel++)
            {
                var deviceOutputViewModel = new DeviceOutputViewModel(device, channel);

                var slider = new ExtendedSlider
                {
                    BindingContext = deviceOutputViewModel,
                    HeightRequest = 50
                };

                slider.SetBinding(ExtendedSlider.ValueProperty, nameof(DeviceOutputViewModel.Output), BindingMode.TwoWay);
                slider.SetBinding(ExtendedSlider.TouchUpCommandProperty, nameof(DeviceOutputViewModel.TouchUpCommand));
                slider.SetBinding(ExtendedSlider.IsEnabledProperty, nameof(DeviceOutputViewModel.Device.DeviceState), BindingMode.Default, new DeviceConnectedToBoolConverter());
                slider.SetBinding(ExtendedSlider.MinimumProperty, nameof(DeviceOutputViewModel.MinValue));
                slider.SetBinding(ExtendedSlider.MaximumProperty, nameof(DeviceOutputViewModel.MaxValue));

                StackLayout.Add(slider);
            }
        }

        private class DeviceOutputViewModel : NotifyPropertyChangedSource
        {
            private int _output;

            public DeviceOutputViewModel(Device device, int channel)
            {
                Device = device;
                Channel = channel;
                Output = 0;

                TouchUpCommand = new Command(() => Output = 0);
            }

            public Device Device { get; }
            public int Channel { get; }

            public int MinValue => -100;
            public int MaxValue => 100;

            public int Output
            {
                get { return _output; }
                set
                {
                    _output = value;
                    Device.SetOutput(Channel, (float)value / MaxValue);
                    RaisePropertyChanged();
                }
            }

            public ICommand TouchUpCommand { get; }
        }
    }
}