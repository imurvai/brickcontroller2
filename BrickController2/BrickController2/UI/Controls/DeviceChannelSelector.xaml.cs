using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeviceChannelSelector : ContentView
	{
		public DeviceChannelSelector()
		{
			InitializeComponent();
            ControlContent.BindingContext = this;

            ChannelSelectCommand = new SafeCommand<int>(channel => SelectedChannel = channel);
		}

        public static BindableProperty DeviceTypeProperty = BindableProperty.Create(nameof(DeviceType), typeof(DeviceType), typeof(DeviceChannelSelector), default(DeviceType), BindingMode.OneWay, null, OnDeviceTypeChanged);
        public static BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(DeviceChannelSelector), 0, BindingMode.TwoWay, null, OnSelectedChannelChanged);

        public DeviceType DeviceType
        {
            get => (DeviceType)GetValue(DeviceTypeProperty);
            set => SetValue(DeviceTypeProperty, value);
        }

        public int SelectedChannel
        {
            get => (int)GetValue(SelectedChannelProperty);
            set => SetValue(SelectedChannelProperty, value);
        }

        ICommand ChannelSelectCommand { get; }

        private static void OnDeviceTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs)
            {
                var deviceType = (DeviceType)newValue;
                dcs.SbrickSection.IsVisible = deviceType == DeviceType.SBrick;
                dcs.BuWizzSection.IsVisible = deviceType == DeviceType.BuWizz || deviceType == DeviceType.BuWizz2;
                dcs.InfraredSection.IsVisible = deviceType == DeviceType.Infrared;
            }
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }
    }
}