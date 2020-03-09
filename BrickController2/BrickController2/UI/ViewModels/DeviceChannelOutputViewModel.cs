using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System.Windows.Input;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    // Copied out from BrickController2.UI.Controls.DeviceOutputTesterControl.DeviceOutputViewModel
    public class DeviceChannelOutputViewModel : NotifyPropertyChangedSource
    {
        private int _output;

        public DeviceChannelOutputViewModel(Device device, int channel)
        {
            Device = device;
            Channel = channel;
            Output = 0;

            TouchUpCommand = new Command(() => Output = 0);
        }

        public Device Device { get; }
        public DeviceType DeviceType => Device.DeviceType;
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
