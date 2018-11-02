using System;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    internal class GattCharacteristic : IGattCharacteristic
    {
        public GattCharacteristic(CBCharacteristic characteristic)
        {
            Characteristic = characteristic;
        }

        public CBCharacteristic Characteristic { get; }
        public Guid Uuid => Characteristic.UUID.ToGuid();
    }
}