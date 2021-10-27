using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;
using CoreFoundation;
using Foundation;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEService : CBCentralManagerDelegate, IBluetoothLEService
    {
        private readonly CBCentralManager _centralManager;
        private readonly IDictionary<CBPeripheral, BluetoothLEDevice> _peripheralMap = new Dictionary<CBPeripheral, BluetoothLEDevice>();
        private readonly object _lock = new object();

        private Action<ScanResult> _scanCallback;

        public BluetoothLEService()
        {
            _centralManager = new CBCentralManager(this, DispatchQueue.CurrentQueue);
        }

        public bool IsBluetoothLESupported => true;
        public bool IsBluetoothOn => _centralManager.State == CBCentralManagerState.PoweredOn;

        public async Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
        {
            if (!IsBluetoothLESupported || !IsBluetoothOn || _centralManager.IsScanning)
            {
                return false;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                lock (_lock)
                {
                    _centralManager.StopScan();
                    _scanCallback = null;
                    tcs.TrySetResult(true);
                }
            }))
            {
                _scanCallback = scanCallback;
                _centralManager.ScanForPeripherals(null, new PeripheralScanningOptions { AllowDuplicatesKey = true });

                return await tcs.Task;
            }
        }

        public IBluetoothLEDevice GetKnownDevice(string address)
        {
            var peripheral = _centralManager?.RetrievePeripheralsWithIdentifiers(new NSUuid(address)).FirstOrDefault();
            if (peripheral == null)
            {
                return null;
            }

            var device = new BluetoothLEDevice(_centralManager, peripheral);
            _peripheralMap[peripheral] = device;

            return device;
        }

        public override void UpdatedState(CBCentralManager central)
        {
        }

        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI)
        {
            lock(_lock)
            {
                if (peripheral == null || peripheral.Identifier == null || string.IsNullOrEmpty(peripheral.Name))
                {
                    return;
                }

                var processedAdvertisementData = ProcessAdvertisementData(advertisementData);
                _scanCallback?.Invoke(new ScanResult(peripheral.Name, peripheral.Identifier.ToString(), processedAdvertisementData));
            }
        }

        public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            var device = _peripheralMap[peripheral];
            device.OnDeviceConnected();
        }

        public override void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            var device = _peripheralMap[peripheral];
            device.OnDeviceDisconnected();
        }

        public override void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            var device = _peripheralMap[peripheral];
            device.OnDeviceDisconnected();
        }

        private IDictionary<byte, byte[]> ProcessAdvertisementData(NSDictionary advertisementData)
        {
            var result = new Dictionary<byte, byte[]>();

            var manufacturerData = GetDataForKey(advertisementData, CBAdvertisement.DataManufacturerDataKey);
            if (manufacturerData != null)
            {
                result[0xFF] = manufacturerData;
            }

            var completeDeviceName = GetDataForKey(advertisementData, CBAdvertisement.DataLocalNameKey);
            if (completeDeviceName != null)
            {
                result[0x09] = completeDeviceName;
            }

            // TODO: add the rest of the advertisementdata...

            return result;
        }

        private byte[] GetDataForKey(NSDictionary advertisementData, NSString key)
        {
            if (advertisementData == null || !advertisementData.ContainsKey(key))
            {
                return null;
            }

            var rawObject = advertisementData[key];
            if (rawObject is NSData dataObject)
            {
                return dataObject.ToArray();
            }
            else if (rawObject is NSString stringObject)
            {
                return Encoding.ASCII.GetBytes(stringObject.ToString());
            }

            return null;
        }
    }
}