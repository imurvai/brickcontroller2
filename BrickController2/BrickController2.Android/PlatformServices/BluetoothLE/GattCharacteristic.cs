using System;
using Android.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    internal class GattCharacteristic : IGattCharacteristic
    {
        public GattCharacteristic(BluetoothGattCharacteristic bluetoothGattCharacteristic)
        {
            BluetoothGattCharacteristic = bluetoothGattCharacteristic;
        }

        public BluetoothGattCharacteristic BluetoothGattCharacteristic { get; }
        public Guid Uuid => BluetoothGattCharacteristic.Uuid!.ToGuid();
    }
}