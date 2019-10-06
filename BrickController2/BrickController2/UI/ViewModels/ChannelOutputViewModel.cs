using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class ChannelOutputViewModel : NotifyPropertyChangedSource
    {
        private int _output;

        public ChannelOutputViewModel(Device device, DevicePort devicePort)
        {
            Device = device;
            DevicePort = devicePort;
            Output = 0;

            TouchUpCommand = new Xamarin.Forms.Command(() => Output = 0);
        }

        public Device Device { get; }
        public DevicePort DevicePort { get; }

        public int Channel => DevicePort.Channel;
        public string Name => DevicePort.Name;

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
