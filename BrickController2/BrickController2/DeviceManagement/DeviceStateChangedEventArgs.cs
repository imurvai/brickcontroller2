using System;

namespace BrickController2.DeviceManagement
{
    public class DeviceStateChangedEventArgs : EventArgs
    {
        internal DeviceStateChangedEventArgs(DeviceState oldState, DeviceState newState, bool isError)
        {
            OldState = oldState;
            NewState = newState;
            IsError = isError;
        }

        public DeviceState OldState { get; }
        public DeviceState NewState { get; }
        public bool IsError { get; }
    }
}
