using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using Plugin.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IAdapter _adapter;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(IAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                try
                {
                    using (_adapter.ScanExtra(new ScanConfig { ScanType = BleScanType.LowLatency })
                        .Subscribe(async scanResult =>
                        {
                            var deviceType = GetDeviceType(scanResult.AdvertisementData);
                            if (deviceType != DeviceType.Unknown)
                            {
                                await deviceFoundCallback(deviceType, scanResult.Device.Name, scanResult.Device.Uuid.ToString());
                            }
                        }))
                    {
                        await Task.Run(() => token.WaitHandle.WaitOne());
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _adapter.StopScan();
                }
            }
        }

        private DeviceType GetDeviceType(IAdvertisementData advertisementData)
        {
            var manufacturerData = advertisementData.ManufacturerData;

            if (manufacturerData == null || manufacturerData.Length < 2)
            {
                return DeviceType.Unknown;
            }

            var data1 = manufacturerData[0];
            var data2 = manufacturerData[1];

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