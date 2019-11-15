﻿using System;
using System.Collections.Generic;
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

        public async Task<bool> ScanAsync(Func<DeviceType, string, string, byte[], Task> deviceFoundCallback, CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!IsBluetoothOn)
                {
                    return true;
                }

                try
                {
                    return await _bleService.ScanDevicesAsync(
                        async scanResult =>
                        {
                            var deviceInfo = GetDeviceIfo(scanResult.AdvertismentData);
                            if (deviceInfo.DeviceType != DeviceType.Unknown)
                            {
                                await deviceFoundCallback(deviceInfo.DeviceType, scanResult.DeviceName, scanResult.DeviceAddress, deviceInfo.ManufacturerData);
                            }
                        },
                        token);
                }
                catch (OperationCanceledException)
                {
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private (DeviceType DeviceType, byte[] ManufacturerData) GetDeviceIfo(IDictionary<byte, byte[]> advertismentData)
        {
            if (advertismentData == null || !advertismentData.ContainsKey(0xFF))
            {
                return (DeviceType.Unknown, null);
            }

            var manufacturerData = advertismentData[0xFF];
            if (manufacturerData == null || manufacturerData.Length < 2)
            {
                return (DeviceType.Unknown, null);
            }

            var manufacturerDataString = BitConverter.ToString(manufacturerData).ToLower();
            var manufacturerId = manufacturerDataString.Substring(0, 5);

            switch (manufacturerId)
            {
                case "98-01": return (DeviceType.SBrick, manufacturerData);
                case "48-4d": return (DeviceType.BuWizz, manufacturerData);
                case "4e-05": return (DeviceType.BuWizz2, manufacturerData);
                case "97-03":
                    if (manufacturerDataString.Length >= 11)
                    {
                        var pupType = manufacturerDataString.Substring(9, 2);
                        switch (pupType)
                        {
                            case "40": return (DeviceType.Boost, manufacturerData);
                            case "41": return (DeviceType.PoweredUp, manufacturerData);
                            case "80": return (DeviceType.TechnicHub, manufacturerData);
                            //case "20": return (DeviceType.DuploTrainHub, manufacturerData);
                        }
                    }
                    break;
            }

            return (DeviceType.Unknown, null);
        }
    }
}