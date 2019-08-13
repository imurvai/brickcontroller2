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

        Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(bool autoConnect, CancellationToken token);
        void Disconnect();

        Task<byte[]> ReadAsync(IGattCharacteristic characteristic, byte[] data);

        Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data);
        bool WriteNoResponse(IGattCharacteristic characteristic, byte[] data);
    }
}
