using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Device = BrickController2.DeviceManagement.Device;

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
            BuWizz3Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            BuWizz3Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BuWizz3Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            BuWizz3Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            BuWizz3Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
            BuWizz3Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
            InfraredChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            InfraredChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            PoweredUpChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            PoweredUpChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            BoostChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
            BoostChannelD.Command = new SafeCommand(() => SelectedChannel = 3);
            TechnicHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            TechnicHubChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicHubChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            TechnicHubChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            DuploTrainHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            CircuitCubesA.Command = new SafeCommand(() => SelectedChannel = 0);
            CircuitCubesB.Command = new SafeCommand(() => SelectedChannel = 1);
            CircuitCubesC.Command = new SafeCommand(() => SelectedChannel = 2);
            WedoChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            WedoChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicMoveChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            TechnicMoveChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicMoveChannelAB.Command = new SafeCommand(() => SelectedChannel = TechnicMoveDevice.CHANNEL_VM);
            TechnicMoveChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
            TechnicMoveChannel1.Command = new SafeCommand(() => SelectedChannel = 3);
            TechnicMoveChannel2.Command = new SafeCommand(() => SelectedChannel = 4);
            TechnicMoveChannel3.Command = new SafeCommand(() => SelectedChannel = 5);
            TechnicMoveChannel4.Command = new SafeCommand(() => SelectedChannel = 6);
            TechnicMoveChannel5.Command = new SafeCommand(() => SelectedChannel = 7);
            TechnicMoveChannel6.Command = new SafeCommand(() => SelectedChannel = 8);
        }

        public static readonly BindableProperty DeviceProperty = BindableProperty.Create(nameof(Device), typeof(Device), typeof(DeviceChannelSelector), default(Device), BindingMode.OneWay, null, OnDeviceChanged);
        public static readonly BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(DeviceChannelSelector), 0, BindingMode.TwoWay, null, OnSelectedChannelChanged);

        public Device Device
        {
            get => (Device)GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        public int SelectedChannel
        {
            get => (int)GetValue(SelectedChannelProperty);
            set => SetValue(SelectedChannelProperty, value);
        }

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs && newValue is Device device)
            {
                var deviceType = device.DeviceType;
                dcs.SbrickSection.IsVisible = deviceType == DeviceType.SBrick;
                dcs.BuWizzSection.IsVisible = deviceType == DeviceType.BuWizz || deviceType == DeviceType.BuWizz2;
                dcs.BuWizz3Section.IsVisible = deviceType == DeviceType.BuWizz3;
                dcs.InfraredSection.IsVisible = deviceType == DeviceType.Infrared;
                dcs.PoweredUpSection.IsVisible = deviceType == DeviceType.PoweredUp;
                dcs.BoostSection.IsVisible = deviceType == DeviceType.Boost;
                dcs.TechnicHubSection.IsVisible = deviceType == DeviceType.TechnicHub;
                dcs.DuploTrainHubSection.IsVisible = deviceType == DeviceType.DuploTrainHub;
                dcs.CircuitCubes.IsVisible = deviceType == DeviceType.CircuitCubes;
                dcs.Wedo2Section.IsVisible = deviceType == DeviceType.WeDo2;
                // Technic Move enablement
                var isPlayVm = device is TechnicMoveDevice moveDevice && moveDevice.EnablePlayVmMode;
                dcs.TechnicMoveSection.IsVisible = deviceType == DeviceType.TechnicMove;
                dcs.TechnicMoveChannelA.IsVisible = !isPlayVm;
                dcs.TechnicMoveChannelB.IsVisible = !isPlayVm;
                dcs.TechnicMoveChannelAB.IsVisible = isPlayVm;
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
                dcs.BuWizz3Channel0.SelectedChannel = selectedChannel;
                dcs.BuWizz3Channel1.SelectedChannel = selectedChannel;
                dcs.BuWizz3Channel2.SelectedChannel = selectedChannel;
                dcs.BuWizz3Channel3.SelectedChannel = selectedChannel;
                dcs.BuWizz3Channel4.SelectedChannel = selectedChannel;
                dcs.BuWizz3Channel5.SelectedChannel = selectedChannel;
                dcs.InfraredChannel0.SelectedChannel = selectedChannel;
                dcs.InfraredChannel1.SelectedChannel = selectedChannel;
                dcs.PoweredUpChannel0.SelectedChannel = selectedChannel;
                dcs.PoweredUpChannel1.SelectedChannel = selectedChannel;
                dcs.BoostChannelA.SelectedChannel = selectedChannel;
                dcs.BoostChannelB.SelectedChannel = selectedChannel;
                dcs.BoostChannelC.SelectedChannel = selectedChannel;
                dcs.BoostChannelD.SelectedChannel = selectedChannel;
                dcs.TechnicHubChannel0.SelectedChannel = selectedChannel;
                dcs.TechnicHubChannel1.SelectedChannel = selectedChannel;
                dcs.TechnicHubChannel2.SelectedChannel = selectedChannel;
                dcs.TechnicHubChannel3.SelectedChannel = selectedChannel;
                dcs.DuploTrainHubChannel0.SelectedChannel = selectedChannel;
                dcs.CircuitCubesA.SelectedChannel = selectedChannel;
                dcs.CircuitCubesB.SelectedChannel = selectedChannel;
                dcs.CircuitCubesC.SelectedChannel = selectedChannel;
                dcs.WedoChannel0.SelectedChannel = selectedChannel;
                dcs.WedoChannel1.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannelA.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannelB.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannelAB.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannelC.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel1.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel2.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel3.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel4.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel5.SelectedChannel = selectedChannel;
                dcs.TechnicMoveChannel6.SelectedChannel = selectedChannel;
            }
        }
    }
}