using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEService : IBluetoothLEService
    {
        private readonly CBCentralManager _centralManager;

        public BluetoothLEService()
        {

        }

        public bool IsBluetoothLESupported => true;

        public bool IsBluetoothOn => throw new NotImplementedException();

        public Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IBluetoothLEDevice GetKnownDeviceAsync(string address)
        {
            throw new NotImplementedException();
        }
    }
}