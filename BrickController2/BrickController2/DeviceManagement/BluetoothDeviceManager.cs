using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BrickController2.DeviceManagement
{
    internal class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IBluetoothLE _ble;
        private readonly IAdapter _adapter;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(IBluetoothLE ble, IAdapter adapter)
        {
            _ble = ble;
            _adapter = adapter;
        }

        public async Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token)
        {
            var deviceDiscoveredHandler = new EventHandler<DeviceEventArgs>(async (sender, args) =>
            {
                var deviceType = GetDeviceType(args.Device);
                if (deviceType != DeviceType.Unknown)
                {
                    await deviceFoundCallback(deviceType, args.Device.Name, args.Device.Id.ToString());
                }
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

                await _adapter.StartScanningForDevicesAsync(null, DeviceFilter, false, CancellationToken.None);
            }
        }

        private bool DeviceFilter(Plugin.BLE.Abstractions.Contracts.IDevice device)
        {
            return GetDeviceType(device) != DeviceType.Unknown;
        }

        private DeviceType GetDeviceType(Plugin.BLE.Abstractions.Contracts.IDevice device)
        {
            var manufacturerData = device.AdvertisementRecords.FirstOrDefault(ar => ar.Type == AdvertisementRecordType.ManufacturerSpecificData);

            if (manufacturerData?.Data == null || manufacturerData.Data.Length < 2)
            {
                return DeviceType.Unknown;
            }

            var data1 = manufacturerData.Data[0];
            var data2 = manufacturerData.Data[1];

            if (data1 == 0x98 && data2 == 0x01)
            {
                return DeviceType.SBrick;
            }

            if (data1 == 0x48 && data2 == 0x4D)
            {
                return DeviceType.BuWizz;
            }

            if (data1 == 0x4e && data2 == 0x05)
            {
                return DeviceType.BuWizz2;
            }

            return DeviceType.Unknown;
        }
    }
}