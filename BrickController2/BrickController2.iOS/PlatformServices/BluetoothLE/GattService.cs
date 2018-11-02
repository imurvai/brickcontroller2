using System;
using System.Collections.Generic;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    internal class GattService : IGattService
    {
        public GattService(CBService service, IEnumerable<IGattCharacteristic> characteristics)
        {
            Service = service;
            Characteristics = characteristics;
        }

        public CBService Service { get; }
        public Guid Uuid => Service.UUID.ToGuid();
        public IEnumerable<IGattCharacteristic> Characteristics { get; }
    }
}