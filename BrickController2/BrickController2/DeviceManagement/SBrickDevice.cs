using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_HARDWARE_REVISION = new Guid("00002a27-0000-1000-8000-00805f9b34fb");
        private readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("4dc591b0-857c-41de-b5f1-15abda665b0c");
        private readonly Guid CHARACTERISTIC_REMOTE_CONTROL = new Guid("02b8cbcc-0e25-4bda-8790-a15f53e6010f");
        private readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");

        private readonly byte[] _sendBuffer = new byte[4];
        private readonly int[] _outputValues = new int[4];

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _remoteControlCharacteristic;
        private IGattCharacteristic _quickDriveCharacteristic;

        public SBrickDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            var firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_FIRMWARE_REVISION);
            var hardwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_HARDWARE_REVISION);

            var remoteControlService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_REMOTE_CONTROL);
            _remoteControlCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_REMOTE_CONTROL);
            _quickDriveCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_QUICK_DRIVE);

            if (firmwareRevisionCharacteristic != null)
            {
                var firmwareData = await _bleDevice?.ReadAsync(firmwareRevisionCharacteristic, token);
                FirmwareVersion = ByteArrayToString(firmwareData);
            }

            if (hardwareRevisionCharacteristic != null)
            {
                var hardwareData = await _bleDevice?.ReadAsync(hardwareRevisionCharacteristic, token);
                HardwareVersion = ByteArrayToString(hardwareData);
            }

            return _remoteControlCharacteristic != null && _quickDriveCharacteristic != null;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                _outputValues[0] = 0;
                _outputValues[1] = 0;
                _outputValues[2] = 0;
                _outputValues[3] = 0;
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

                while (!token.IsCancellationRequested)
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        int v0 = _outputValues[0];
                        int v1 = _outputValues[1];
                        int v2 = _outputValues[2];
                        int v3 = _outputValues[3];

                        if (await SendOutputValuesAsync(v0, v1, v2, v3, token))
                        {
                            if (v0 != 0 || v1 != 0 || v2 != 0 || v3 != 0)
                            {
                                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                            }
                            else
                            {
                                _sendAttemptsLeft--;
                            }
                        }
                        else
                        {
                            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token);
                    }
                }
            }
            catch
            {
            }
        }

        private string ByteArrayToString(byte[] data)
        {
            try
            {
                if (data != null)
                {
                    return Encoding.ASCII.GetString(data);
                }
            }
            catch
            {
            }

            return "-";
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                _sendBuffer[0] = (byte)((Math.Abs(v0) & 0xfe) | 0x02 | (v0 < 0 ? 1 : 0));
                _sendBuffer[1] = (byte)((Math.Abs(v1) & 0xfe) | 0x02 | (v1 < 0 ? 1 : 0));
                _sendBuffer[2] = (byte)((Math.Abs(v2) & 0xfe) | 0x02 | (v2 < 0 ? 1 : 0));
                _sendBuffer[3] = (byte)((Math.Abs(v3) & 0xfe) | 0x02 | (v3 < 0 ? 1 : 0));

                return await _bleDevice?.WriteAsync(_quickDriveCharacteristic, _sendBuffer, token);
            }
            catch
            {
                return false;
            }
        }
    }
}
