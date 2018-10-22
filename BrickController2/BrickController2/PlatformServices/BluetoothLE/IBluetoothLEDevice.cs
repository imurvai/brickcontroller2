using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IBluetoothLEDevice
    {
        string Address { get; }
        BluetoothLEDeviceState State { get; }
        IDictionary<string, IEnumerable<string>> ServicesAndCharacteristics { get; }

        event EventHandler<BluetoothLEDeviceStateChangedEventArgs> StateChanged;

        Task<bool> ConnectAndDiscoverServicesAsync(CancellationToken token);
        Task DisconnectAsync();

        bool Write(string characteristic, byte[] data, bool noResponse = false);
    }
}
