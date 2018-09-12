using BrickController2.DeviceManagement;
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

        private void OnDeviceTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        private void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }
    }
}