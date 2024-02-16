using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BuWizz2Device : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private static readonly Guid SERVICE_UUID = new Guid("4e050000-74fb-4481-88b3-9919b1676e93");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("000092d1-0000-1000-8000-00805f9b34fb");

        private static readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_MODEL_NUMBER = new Guid("00002a24-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");

        private static readonly TimeSpan VoltageMeasurementTimeout = TimeSpan.FromSeconds(5);

        private readonly int[] _outputValues = new int[4];
        private readonly int[] _lastOutputValues = new int[4];
        private readonly object _outputLock = new object();
        private readonly bool _swapChannels;

        private DateTime _batteryMeasurementTimestamp;
        private byte _batteryVoltageRaw;
        private byte _motorVoltageRaw;

        private volatile int _outputLevelValue;
        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;
        private IGattCharacteristic _modelNumberCharacteristic;
        private IGattCharacteristic _firmwareRevisionCharacteristic;

        public BuWizz2Device(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            // On BuWizz2 with manufacturer data 0x4e054257001e the ports are swapped
            // (no normal BuWizz2es manufacturer data is 0x4e054257001b)
            _swapChannels = deviceData != null && deviceData.Length >= 6 && deviceData[5] == 0x1E;
        }

        public override DeviceType DeviceType => DeviceType.BuWizz2;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 4;
        public override int DefaultOutputLevel => 1;
        protected override bool AutoConnectOnFirstConnect => false;

        public override string BatteryVoltageSign => "V";

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

        public override bool CanSetOutputLevel => true;

        public override void SetOutputLevel(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
        }

        public override bool CanBePowerSource => true;

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
            if (characteristicGuid != _characteristic.Uuid || data.Length < 4 || data[0] != 0x00)
            {
                return;
            }

            // Byte 1: Status flags - Bits 3-4 Battery level status (0 - empty, motors disabled; 1 - low; 2 - medium; 3 - full) 

            // do some change filtering as data are comming at 25Hz frequency
            if (GetVoltage(data, out float batteryVoltage, out float motorVoltage))
            {
                BatteryVoltage = $"{batteryVoltage:F2} / {motorVoltage:F2}";
            }
        }

        private bool GetVoltage(byte[] data, out float batteryVoltage, out float motorVoltage)
        {
            // Byte 2: Battery voltage (3 V + value * 0,01 V) -range 3,00 V - 4,27 V
            // Byte 3: Output(motor) voltage(4 V + value * 0,05 V) - range 4,00 V - 16,75 V
            batteryVoltage = 3.0f + data[2] * 0.01f;
            motorVoltage = 4.0f + data[3] * 0.05f;

            const int delta = 2;

            if (Math.Abs(_batteryVoltageRaw - data[2]) > delta ||
                Math.Abs(_motorVoltageRaw - data[3]) > delta ||
                DateTime.Now - _batteryMeasurementTimestamp > VoltageMeasurementTimeout)
            {
                _batteryVoltageRaw = data[2];
                _motorVoltageRaw = data[3];

                _batteryMeasurementTimestamp = DateTime.Now;

                return true;
            }

            return false;
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
                    _lastOutputValues[0] = 1;
                    _lastOutputValues[1] = 1;
                    _lastOutputValues[2] = 1;
                    _lastOutputValues[3] = 1;
                    _outputLevelValue = DefaultOutputLevel;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                var _lastSentOutputLevelValue = -1;

                while (!token.IsCancellationRequested)
                {
                    if (_lastSentOutputLevelValue != _outputLevelValue)
                    {
                        if (await SendOutputLevelValueAsync(_outputLevelValue, token))
                        {
                            _lastSentOutputLevelValue = _outputLevelValue;
                        }
                    }
                    else
                    {
                        int v0, v1, v2, v3, sendAttemptsLeft;

                        lock (_outputLock)
                        {
                            v0 = _outputValues[0];
                            v1 = _outputValues[1];
                            v2 = _outputValues[2];
                            v3 = _outputValues[3];
                            sendAttemptsLeft = _sendAttemptsLeft;
                            _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                        }

                        if (v0 != _lastOutputValues[0] || v1 != _lastOutputValues[1] || v2 != _lastOutputValues[2] || v3 != _lastOutputValues[3] || sendAttemptsLeft > 0)
                        {
                            if (await SendOutputValuesAsync(v0, v1, v2, v3, token).ConfigureAwait(false))
                            {
                                _lastOutputValues[0] = v0;
                                _lastOutputValues[1] = v1;
                                _lastOutputValues[2] = v2;
                                _lastOutputValues[3] = v3;
                            }
                        }
                        else
                        {
                            await Task.Delay(10, token).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                var sendOutputBuffer = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 };

                if (_swapChannels)
                {
                    sendOutputBuffer[1] = (byte)(v1 / 2);
                    sendOutputBuffer[2] = (byte)(v0 / 2);
                    sendOutputBuffer[3] = (byte)(v3 / 2);
                    sendOutputBuffer[4] = (byte)(v2 / 2);
                }
                else
                {
                    sendOutputBuffer[1] = (byte)(v0 / 2);
                    sendOutputBuffer[2] = (byte)(v1 / 2);
                    sendOutputBuffer[3] = (byte)(v2 / 2);
                    sendOutputBuffer[4] = (byte)(v3 / 2);
                }

                return await _bleDevice?.WriteAsync(_characteristic, sendOutputBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendOutputLevelValueAsync(int outputLevelValue, CancellationToken token)
        {
            try
            {
                var sendOutputLevelBuffer = new byte[] { 0x11, (byte)(outputLevelValue + 1) };

                return await _bleDevice?.WriteAsync(_characteristic, sendOutputLevelBuffer, token);
            }
            catch (Exception)
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

            var modelNumberData = await _bleDevice?.ReadAsync(_modelNumberCharacteristic, token);
            var modelNumber = modelNumberData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(modelNumber))
            {
                HardwareVersion = modelNumber;
            }
        }
    }
}
