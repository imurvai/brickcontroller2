using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizz3Device : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private static readonly Guid SERVICE_UUID = new Guid("500592d1-74fb-4481-88b3-9919b1676e93");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("50052901-74fb-4481-88b3-9919b1676e93");

        private static readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_MODEL_NUMBER = new Guid("00002a24-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");

        private static readonly TimeSpan VoltageMeasurementTimeout = TimeSpan.FromSeconds(5);

        private readonly int[] _outputValues = new int[6];
        private readonly int[] _lastOutputValues = new int[6];
        private readonly object _outputLock = new object();

        private DateTime _batteryMeasurementTimestamp;
        private byte _batteryVoltageRaw;

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;
        private IGattCharacteristic _modelNumberCharacteristic;
        private IGattCharacteristic _firmwareRevisionCharacteristic;

        public BuWizz3Device(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz3;
        public override int NumberOfChannels => 6;
        public override int NumberOfOutputLevels => 1;
        public override int DefaultOutputLevel => 0;
        protected override bool AutoConnectOnFirstConnect => false;

        public override string BatteryVoltageSign => "V";

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 127);

            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        public override bool CanBePowerSource => false;

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_FIRMWARE_REVISION);
            _modelNumberCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_MODEL_NUMBER);

            if (_characteristic != null)
            {
                await _bleDevice?.EnableNotificationAsync(_characteristic, token);
            }

            return _characteristic != null && _firmwareRevisionCharacteristic != null && _modelNumberCharacteristic != null;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != _characteristic.Uuid || data.Length < 3 || data[0] != 0x01)
            {
                return;
            }

            // Byte 1: Status flags - Bits 3-4 Battery level status (0 - empty, motors disabled; 1 - low; 2 - medium; 3 - full) 

            // do some change filtering as data are comming at 20Hz frequency
            if (GetVoltage(data, out float batteryVoltage))
            {
                BatteryVoltage = $"{batteryVoltage:F2}";
            }
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    await ReadDeviceInfo(token);
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
                    _outputValues[4] = 0;
                    _outputValues[5] = 0;
                    _lastOutputValues[0] = 1;
                    _lastOutputValues[1] = 1;
                    _lastOutputValues[2] = 1;
                    _lastOutputValues[3] = 1;
                    _lastOutputValues[4] = 1;
                    _lastOutputValues[5] = 1;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                while (!token.IsCancellationRequested)
                {
                    int v0, v1, v2, v3, v4, v5, sendAttemptsLeft;

                    lock (_outputLock)
                    {
                        v0 = _outputValues[0];
                        v1 = _outputValues[1];
                        v2 = _outputValues[2];
                        v3 = _outputValues[3];
                        v4 = _outputValues[4];
                        v5 = _outputValues[5];
                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }

                    if (v0 != _lastOutputValues[0] ||
                        v1 != _lastOutputValues[1] ||
                        v2 != _lastOutputValues[2] ||
                        v3 != _lastOutputValues[3] ||
                        v4 != _lastOutputValues[4] ||
                        v5 != _lastOutputValues[5] ||
                        sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(v0, v1, v2, v3, v4, v5, token).ConfigureAwait(false))
                        {
                            _lastOutputValues[0] = v0;
                            _lastOutputValues[1] = v1;
                            _lastOutputValues[2] = v2;
                            _lastOutputValues[3] = v3;
                            _lastOutputValues[4] = v4;
                            _lastOutputValues[5] = v5;
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, int v4, int v5, CancellationToken token)
        {
            try
            {
                var sendOutputBuffer = new byte[] { 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                sendOutputBuffer[1] = (byte)v0;
                sendOutputBuffer[2] = (byte)v1;
                sendOutputBuffer[3] = (byte)v2;
                sendOutputBuffer[4] = (byte)v3;
                sendOutputBuffer[5] = (byte)v4;
                sendOutputBuffer[6] = (byte)v5;

                return await _bleDevice?.WriteAsync(_characteristic, sendOutputBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool GetVoltage(byte[] data, out float batteryVoltage)
        {
            // Battery voltage(9 V + value * 0,05 V) - range 9,00 V – 15,35 V
            batteryVoltage = 9.0f + data[2] * 0.05f;

            const int delta = 2;

            if (Math.Abs(_batteryVoltageRaw - data[2]) > delta ||
                DateTime.Now - _batteryMeasurementTimestamp > VoltageMeasurementTimeout)
            {
                _batteryVoltageRaw = data[2];
                _batteryMeasurementTimestamp = DateTime.Now;

                return true;
            }

            return false;
        }

        private async Task ReadDeviceInfo(CancellationToken token)
        {
            var firmwareData = await _bleDevice?.ReadAsync(_firmwareRevisionCharacteristic, token);
            var firmwareVersion = firmwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(firmwareVersion))
            {
                FirmwareVersion = firmwareVersion;
            }

            var modelNumberData = await _bleDevice?.ReadAsync(_modelNumberCharacteristic, token);
            var modelNumber = modelNumberData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(modelNumber))
            {
                HardwareVersion = modelNumber;
            }
        }
    }
}
