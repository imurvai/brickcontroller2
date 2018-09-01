using System;

namespace BrickController2.DeviceManagement
{
    public class DeviceStateChangedEventArgs : EventArgs
    {
        public DeviceStateChangedEventArgs(DeviceState deviceState)
        {
            DeviceState = deviceState;
        }

        public DeviceState DeviceState { get; }
    }
}
