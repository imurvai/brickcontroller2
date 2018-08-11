using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.DeviceManagement
{
    public class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IBluetoothLE _ble;
        private readonly IAdapter _adapter;

        public BluetoothDeviceManager(IBluetoothLE ble, IAdapter adapter)
        {
            _ble = ble;
            _adapter = adapter;

            _adapter.DeviceDiscovered += (sender, args) =>
            {
                Console.WriteLine($"Found device: {args.Device.Name} - {args.Device.Id}");
            };
        }

        public async Task ScanAsync(CancellationToken token)
        {
            token.Register(async () =>
            {
                await _adapter.StopScanningForDevicesAsync();
            });

            await _adapter.StartScanningForDevicesAsync();
        }
    }
}