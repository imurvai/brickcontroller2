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

        Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(
            bool autoConnect,
            Action<Guid, byte[]> onCharacteristicChanged,
            Action<IBluetoothLEDevice> onDeviceDisconnected,
            CancellationToken token);
        Task DisconnectAsync();

        Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token);

        Task<byte[]> ReadAsync(IGattCharacteristic characteristic, CancellationToken token);

        Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token);
        Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token);
    }
}
