using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;
using Foundation;

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
            if (!IsBluetoothLESupported || !IsBluetoothOn)
            {
                return Task.FromResult(false);
            }

            throw new NotImplementedException();
        }

        public IBluetoothLEDevice GetKnownDeviceAsync(string address)
        {
            var device = _centralManager?.RetrievePeripheralsWithIdentifiers(new NSUuid(address)).FirstOrDefault();
            if (device == null)
            {
                return null;
            }

            return new BluetoothLEDevice(address);
        }
    }
}