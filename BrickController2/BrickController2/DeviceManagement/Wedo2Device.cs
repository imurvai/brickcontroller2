using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class Wedo2Device : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 3;

        internal static readonly Guid SERVICE_UUID = new Guid("00001523-1212-efde-1523-785feabcd123");

        private static readonly Guid CONTROL_SERVICE_UUID = new Guid("00004f0e-1212-efde-1523-785feabcd123");
        private static readonly Guid SENSOR_VALUE_CHARACTERISTIC_UUID = new Guid("00001560-1212-efde-1523-785feabcd123");
        private static readonly Guid INPUT_CHARACTERISTIC_UUID = new Guid("00001563-1212-efde-1523-785feabcd123");
        private static readonly Guid OUTPUT_CHARACTERISTIC_UUID = new Guid("00001565-1212-efde-1523-785feabcd123");

        private static readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");

        // Motor Driving Commands
        private readonly byte[] _motorBuffer = new byte[] { 0x00, 0x01, 0x01, 0x00 };

        private readonly byte[] _outputValues = new byte[2];
        private readonly byte[] _lastOutputValues = new byte[2];
        private readonly object _outputLock = new object();

        private readonly int[] _sendAttemptsLeft = new int[2];

        private IGattCharacteristic _motorCharacteristic;
        private IGattCharacteristic _sensorValueCharacteristic;
        private IGattCharacteristic _inputCharacteristic;
        private IGattCharacteristic _firmwareRevisionCharacteristic;

        public Wedo2Device(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        protected override bool AutoConnectOnFirstConnect => false;

        public override DeviceType DeviceType => DeviceType.WeDo2;

        public override int NumberOfChannels => 2;

        public override string BatteryVoltageSign => "V";

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            // Per channel range of 0 to 100
            var rawValue = value < 0 ?
                (byte) (256 + value * 100) : 
                (byte)(value * 100);

            lock (_outputLock)
            {
                if (_outputValues[channel] != rawValue)
                {
                    _outputValues[channel] = rawValue;
                    _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                }
            }
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == CONTROL_SERVICE_UUID);
            _motorCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == OUTPUT_CHARACTERISTIC_UUID);
            _sensorValueCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == SENSOR_VALUE_CHARACTERISTIC_UUID);
            _inputCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == INPUT_CHARACTERISTIC_UUID);

            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_FIRMWARE_REVISION);

            return Task.FromResult(_motorCharacteristic != null &&
                _inputCharacteristic != null &&
                _sensorValueCharacteristic != null &&
                _firmwareRevisionCharacteristic != null);
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != SENSOR_VALUE_CHARACTERISTIC_UUID || data.Length < 2)
                return;

            // voltage sensor on port #4 in SI units
            if (data[1] == 0x04 && data[0] == 0x02)
            {
                var voltage = 0.001f * data.GetFloat(2);
                BatteryVoltage = $"{voltage:F2}";
            }
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    var firmwareData = await _bleDevice?.ReadAsync(_firmwareRevisionCharacteristic, token);
                    var firmwareVersion = firmwareData?.ToAsciiStringSafe();
                    FirmwareVersion = firmwareVersion ?? String.Empty;
                    BatteryVoltage = String.Empty;

                    // INPUT: INPUT_FORMAT, COMMAND_TYPE_WRITE, port, TYPE, 0, 30i, INPUT_FORMAT_UNIT, 1
                    byte[] voltageCommand = new byte[] { 0x01, 0x02, 0x04, 0x14, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x02, 0x01 };
                    await _bleDevice.WriteAsync(_inputCharacteristic, voltageCommand, token);

                    await _bleDevice.EnableNotificationAsync(_sensorValueCharacteristic, token);
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
                    for (byte channel = 0; channel < NumberOfChannels; channel++)
                    {
                        _outputValues[channel] = 0;
                        _lastOutputValues[channel] = 1;
                        _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                    }
                }

                while (!token.IsCancellationRequested)
                {
                    if (!await SendOutputValuesAsync(token).ConfigureAwait(false))
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch { }
        }

        private async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                for (byte channel = 0; channel < NumberOfChannels; channel++)
                {
                    byte value;
                    bool canSend;

                    lock (_outputLock)
                    {
                        value = _outputValues[channel];
                        canSend = value != _lastOutputValues[channel] || _sendAttemptsLeft[channel] > 0;
                        _sendAttemptsLeft[channel] = Math.Max(0, _sendAttemptsLeft[channel] - 1);
                    }

                    if (canSend)
                    {
                        _motorBuffer[0] = (byte)(1 + channel);
                        _motorBuffer[3] = value;

                        if (!await _bleDevice?.WriteAsync(_motorCharacteristic, _motorBuffer, token))
                        {
                            return false;
                        }

                        _lastOutputValues[channel] = value;
                    }
                    await Task.Delay(5, token);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
