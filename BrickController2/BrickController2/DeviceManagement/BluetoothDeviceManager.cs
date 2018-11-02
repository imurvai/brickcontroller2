using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IBluetoothLEService _bleService;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(IBluetoothLEService bleService)
        {
            _bleService = bleService;
        }

        public bool IsBluetoothLESupported => _bleService.IsBluetoothLESupported;
        public bool IsBluetoothOn => _bleService.IsBluetoothOn;

        public async Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!IsBluetoothOn)
                {
                    return;
                }

                try
                {
                    await _bleService.ScanDevicesAsync(
                        async scanResult =>
                        {
                            var deviceType = GetDeviceType(scanResult.ScanRecord);
                            if (deviceType != DeviceType.Unknown)
                            {
                                await deviceFoundCallback(deviceType, scanResult.DeviceName, scanResult.DeviceAddress);
                            }
                        },
                        token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private DeviceType GetDeviceType(byte[] scanRecord)
        {
            if (scanRecord == null || scanRecord.Length < 2)
            {
                return DeviceType.Unknown;
            }

            var data1 = scanRecord[0];
            var data2 = scanRecord[1];

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

            if (data1 == 0x97 && data2 == 0x03)
            {
                if (scanRecord.Length >= 4)
                {
                    if (scanRecord[3] == 0x40)
                    {
                        //return DeviceType.Boost;
                    }
                    else if (scanRecord[3] == 0x41)
                    {
                        return DeviceType.PoweredUp;
                    }
                }
            }

            return DeviceType.Unknown;
        }
    }
}