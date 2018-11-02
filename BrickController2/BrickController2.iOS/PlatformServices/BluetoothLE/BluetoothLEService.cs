using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly CBPeripheralManager _peripheralManager;
        private readonly IDictionary<CBPeripheral, BluetoothLEDevice> _peripheralMap = new Dictionary<CBPeripheral, BluetoothLEDevice>();
        private readonly object _lock = new object();

        private Action<ScanResult> _scanCallback;

        public BluetoothLEService()
        {
            _centralManager = new CBCentralManager(this, DispatchQueue.CurrentQueue, new CBCentralInitOptions());
            _peripheralManager = new CBPeripheralManager();
        }

        public bool IsBluetoothLESupported => true;
        public bool IsBluetoothOn => _centralManager.State == CBCentralManagerState.PoweredOn;

        public Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
        {
            if (!IsBluetoothLESupported || !IsBluetoothOn || _centralManager.IsScanning)
            {
                return Task.FromResult(false);
            }

            _scanCallback = scanCallback;
            _centralManager.ScanForPeripherals((CBUUID)null);

            var tcs = new TaskCompletionSource<bool>();

            token.Register(() =>
            {
                lock(_lock)
                {
                    _centralManager.StopScan();
                    _scanCallback = null;
                    tcs.SetResult(true);
                }
            });

            return tcs.Task;
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
                byte[] scanRecord = null;
                if (advertisementData != null && advertisementData.ContainsKey(CBAdvertisement.DataManufacturerDataKey))
                {
                    scanRecord = (advertisementData.ObjectForKey(CBAdvertisement.DataManufacturerDataKey) as NSData)?.ToArray();
                }

                _scanCallback?.Invoke(new ScanResult(peripheral.Name, peripheral.UUID.ToString(), scanRecord));
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
    }
}