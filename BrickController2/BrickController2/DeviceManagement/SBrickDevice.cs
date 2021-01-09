﻿using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_UUID_HARDWARE_REVISION = new Guid("00002a27-0000-1000-8000-00805f9b34fb");
        private readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("4dc591b0-857c-41de-b5f1-15abda665b0c");
        private readonly Guid CHARACTERISTIC_UUID_REMOTE_CONTROL = new Guid("02b8cbcc-0e25-4bda-8790-a15f53e6010f");
        private readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");

        private readonly int[] _outputValues = new int[4];
        private int[] _lastDir = new int[4] { 0, 0, 0, 0 };

        private readonly object _outputLock = new object();

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _firmwareRevisionCharacteristic;
        private IGattCharacteristic _hardwareRevisionCharacteristic;
        private IGattCharacteristic _remoteControlCharacteristic;
        private IGattCharacteristic _quickDriveCharacteristic;

        public SBrickDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override string BatteryVoltageSign => "V";
        public override int NumberOfChannels => 4;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            
            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_FIRMWARE_REVISION);
            _hardwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_HARDWARE_REVISION);

            var remoteControlService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_REMOTE_CONTROL);
            _remoteControlCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_REMOTE_CONTROL);
            _quickDriveCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_QUICK_DRIVE);

            return Task.FromResult(
                _firmwareRevisionCharacteristic != null &&
                _hardwareRevisionCharacteristic != null &&
                _remoteControlCharacteristic != null && 
                _quickDriveCharacteristic != null);
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    await ReadDeviceInfo(token).ConfigureAwait(false);
                }
            }
            catch { }

            return true;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                lock (_outputLock)
                {
                    _outputValues[0] = 0;
                    _outputValues[1] = 0;
                    _outputValues[2] = 0;
                    _outputValues[3] = 0;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                while (!token.IsCancellationRequested)
                {
                    int sendAttemptsLeft;
                    int[] v = new int[NumberOfChannels];
                    bool nonZeroOutput = false;
                    lock (_outputLock)
                    {
                        for (int i = 0; i < NumberOfChannels; i++)
                        {
                            v[i] = _outputValues[i];
                            nonZeroOutput = nonZeroOutput || v[i] != 0;
                        }
                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }
                    if (nonZeroOutput || sendAttemptsLeft > 0)
                    {
                        await SendOutputValuesAsync(v, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch { }
        }

        private async Task<bool> SendOutputValuesAsync(int[] v, CancellationToken token)
        {
            try
            {
                byte[] sendOutputBuffer = new byte[NumberOfChannels];
                for (int i = 0; i < NumberOfChannels; i++) {
                    int dir = v[i] == 0 ? _lastDir[i] : v[i] < 0 ? 1 : 0;
                    sendOutputBuffer[i] = (byte)((Math.Abs(v[i]) & 0xfe) | 0x02 | dir);
                    _lastDir[i] = dir;

                }
                

                return await _bleDevice?.WriteAsync(_quickDriveCharacteristic, sendOutputBuffer, token);
            }
            catch
            {
                return false;
            }
        }

        private async Task ReadDeviceInfo(CancellationToken token)
        {
            var firmwareData = await _bleDevice?.ReadAsync(_firmwareRevisionCharacteristic, token);
            var firmwareVersion = firmwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(firmwareVersion))
            {
                FirmwareVersion = firmwareVersion;
            }

            var hardwareData = await _bleDevice?.ReadAsync(_hardwareRevisionCharacteristic, token);
            var hardwareVersion = hardwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(hardwareVersion))
            {
                HardwareVersion = hardwareVersion;
            }

            await _bleDevice?.WriteAsync(_remoteControlCharacteristic, new byte[] { 0x0f, 0x08 }, token);
            var voltageBuffer = await _bleDevice?.ReadAsync(_remoteControlCharacteristic, token);
            if (voltageBuffer != null && voltageBuffer.Length >= 2)
            {
                var rawVoltage = voltageBuffer[0] + (voltageBuffer[1] << 8);
                var voltage = (rawVoltage * 0.83875F) / 2047;
                BatteryVoltage = voltage.ToString("F2");
            }
        }
    }
}
