using BrickController2.DeviceManagement;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class DeviceOutputViewModel : NotifyPropertyChangedSource
    {
        public DeviceOutputViewModel(Device device, int channel)
        {
            Device = device;
            Channel = channel;
        }

        public Device Device { get; }
        public int Channel { get; }
        public int Output { get; set; }
    }
}
