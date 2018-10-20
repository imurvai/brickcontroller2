using BrickController2.Helpers;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class DeviceOutputViewModel : NotifyPropertyChangedSource
    {
        private int _output;

        public DeviceOutputViewModel(DeviceManagement.Device device, int channel)
        {
            Device = device;
            Channel = channel;
            Output = 0;

            TouchUpCommand = new Command(() => Output = 0);
        }

        public DeviceManagement.Device Device { get; }
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
