using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : IBluetoothLEDevice
    {
        public BluetoothLEDevice(string address)
        {
            Address = address;
        }

        public string Address { get; }
        public BluetoothLEDeviceState State { get; } = BluetoothLEDeviceState.Disconnected;
        public IDictionary<string, IEnumerable<string>> ServicesAndCharacteristics { get; } = new Dictionary<string, IEnumerable<string>>();

        public event EventHandler<BluetoothLEDeviceStateChangedEventArgs> StateChanged;

        public Task<bool> ConnectAndDiscoverServicesAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public bool Write(string characteristic, byte[] data, bool noResponse = false)
        {
            throw new NotImplementedException();
        }
    }
}