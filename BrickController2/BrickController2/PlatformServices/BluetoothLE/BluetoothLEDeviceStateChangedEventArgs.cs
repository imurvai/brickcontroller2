using System;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public class BluetoothLEDeviceStateChangedEventArgs : EventArgs
    {
        public BluetoothLEDeviceStateChangedEventArgs(IBluetoothLEDevice device, BluetoothLEDeviceState oldState, BluetoothLEDeviceState newState)
        {
            Device = device;
            OldState = oldState;
            NewState = newState;
        }

        public IBluetoothLEDevice Device { get; }
        public BluetoothLEDeviceState OldState { get; }
        public BluetoothLEDeviceState NewState { get; }
    }
}
