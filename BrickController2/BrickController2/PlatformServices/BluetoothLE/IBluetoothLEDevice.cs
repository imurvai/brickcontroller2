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

        event EventHandler<EventArgs> Disconnected;

        Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(CancellationToken token);
        Task DisconnectAsync();

        Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data);
        Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data);
    }
}
