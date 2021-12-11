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
                dcs.BuWizz3Section.IsVisible = deviceType == DeviceType.BuWizz3;
                dcs.InfraredSection.IsVisible = deviceType == DeviceType.Infrared;
                dcs.PoweredUpSection.IsVisible = deviceType == DeviceType.PoweredUp;
                dcs.BoostSection.IsVisible = deviceType == DeviceType.Boost;
                dcs.TechnicHubSection.IsVisible = deviceType == DeviceType.TechnicHub;
                dcs.DuploTrainHubSection.IsVisible = deviceType == DeviceType.DuploTrainHub;
                dcs.CircuitCubes.IsVisible = deviceType == DeviceType.CircuitCubes;
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
            }
        }
    }
}