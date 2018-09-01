using BrickController2.DeviceManagement;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class DeviceOutputViewModel : NotifyPropertyChangedSource
    {
        private int _output;

        public DeviceOutputViewModel(Device device, int channel)
        {
            Device = device;
            Channel = channel;
            Output = 0;
        }

        public Device Device { get; }
        public int Channel { get; }

        public int Output
        {
            get { return _output; }
            set
            {
                _output = value;
                //Device.SetOutputAsync(Channel, value);
                RaisePropertyChanged();
            }
        }
    }
}
