using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BrickController2.DeviceManagement
{
    public class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IBluetoothLE _ble;
        private readonly IAdapter _adapter;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(IBluetoothLE ble, IAdapter adapter)
        {
            _ble = ble;
            _adapter = adapter;
        }

        public async Task ScanAsync(Action<Device> deviceFoundCallback, CancellationToken token)
        {
            var deviceDiscoveredHandler = new EventHandler<DeviceEventArgs>((object sender, DeviceEventArgs args) =>
            {
                // TODO: call the deviceFoundCallback here
                Debug.WriteLine($"Found device: {args.Device.Name} - {args.Device.Id}");
            });

            using (await _asyncLock.LockAsync())
            {
                _adapter.ScanMode = ScanMode.LowLatency;
                _adapter.DeviceDiscovered += deviceDiscoveredHandler;

                token.Register(async () =>
                {
                    await _adapter.StopScanningForDevicesAsync();
                    _adapter.DeviceDiscovered -= deviceDiscoveredHandler;
                });

                await _adapter.StartScanningForDevicesAsync();
            }
        }
    }
}