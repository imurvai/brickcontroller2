using System;

namespace BrickController2.HardwareServices.Bluetooth
{
    public class BleDeviceStateChangedEventArgs : EventArgs
    {
        public BleDeviceStateChangedEventArgs(BleDeviceState oldState, BleDeviceState newState, bool isOk)
        {
            OldState = oldState;
            NewState = newState;
            IsOk = isOk;
        }

        public BleDeviceState OldState { get; }
        public BleDeviceState NewState { get; }
        public bool IsOk { get; }
    }
}