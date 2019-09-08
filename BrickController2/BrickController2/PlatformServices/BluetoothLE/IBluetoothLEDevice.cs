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
        void Disconnect();

        bool EnableNotification(IGattCharacteristic characteristic);

        Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token);
        bool WriteNoResponse(IGattCharacteristic characteristic, byte[] data);
    }
}
