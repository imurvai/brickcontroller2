using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
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

            SBrickChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            SBrickChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            SBrickChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            SBrickChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            BuWizzChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            BuWizzChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BuWizzChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            BuWizzChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            InfraredChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            InfraredChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            PoweredUpChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            PoweredUpChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            BoostChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
            BoostChannelD.Command = new SafeCommand(() => SelectedChannel = 3);
            ControlPlusChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            ControlPlusChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            ControlPlusChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            ControlPlusChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
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

        private static void OnDeviceTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs)
            {
                var deviceType = (DeviceType)newValue;
                dcs.SbrickSection.IsVisible = deviceType == DeviceType.SBrick;
                dcs.BuWizzSection.IsVisible = deviceType == DeviceType.BuWizz || deviceType == DeviceType.BuWizz2;
                dcs.InfraredSection.IsVisible = deviceType == DeviceType.Infrared;
                dcs.PoweredUpSection.IsVisible = deviceType == DeviceType.PoweredUp;
                dcs.BoostSection.IsVisible = deviceType == DeviceType.Boost;
                dcs.ControlPlusSection.IsVisible = deviceType == DeviceType.ControlPlus;
            }
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs)
            {
                int selectedChannel = (int)newValue;
                dcs.SBrickChannel0.SelectedChannel = selectedChannel;
                dcs.SBrickChannel1.SelectedChannel = selectedChannel;
                dcs.SBrickChannel2.SelectedChannel = selectedChannel;
                dcs.SBrickChannel3.SelectedChannel = selectedChannel;
                dcs.BuWizzChannel0.SelectedChannel = selectedChannel;
                dcs.BuWizzChannel1.SelectedChannel = selectedChannel;
                dcs.BuWizzChannel2.SelectedChannel = selectedChannel;
                dcs.BuWizzChannel3.SelectedChannel = selectedChannel;
                dcs.InfraredChannel0.SelectedChannel = selectedChannel;
                dcs.InfraredChannel1.SelectedChannel = selectedChannel;
                dcs.PoweredUpChannel0.SelectedChannel = selectedChannel;
                dcs.PoweredUpChannel1.SelectedChannel = selectedChannel;
                dcs.BoostChannelA.SelectedChannel = selectedChannel;
                dcs.BoostChannelB.SelectedChannel = selectedChannel;
                dcs.BoostChannelC.SelectedChannel = selectedChannel;
                dcs.BoostChannelD.SelectedChannel = selectedChannel;
                dcs.ControlPlusChannel0.SelectedChannel = selectedChannel;
                dcs.ControlPlusChannel1.SelectedChannel = selectedChannel;
                dcs.ControlPlusChannel2.SelectedChannel = selectedChannel;
                dcs.ControlPlusChannel3.SelectedChannel = selectedChannel;
            }
        }
    }
}