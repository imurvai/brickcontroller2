using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IBluetoothLEService
    {
        bool IsBluetoothLESupported { get; }
        bool IsBluetoothOn { get; }

        Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token);

        IBluetoothLEDevice GetKnownDevice(string address);
    }
}
