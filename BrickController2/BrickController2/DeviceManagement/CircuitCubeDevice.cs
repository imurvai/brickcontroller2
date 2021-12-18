using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class CircuitCubeDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 5;

        internal static readonly Guid SERVICE_UUID = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        private static readonly Guid CHARACTERISTIC_UUID_WRITE = new Guid("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
        private static readonly Guid CHARACTERISTIC_UUID_NOTIFY = new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e");

        private static readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_HARDWARE_REVISION = new Guid("00002a27-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");

        // Turn off power to all motors command: <0>
        private static readonly byte[] TURN_OFF_ALL_COMMAND = new[] { (byte)'0' };
        // Battery Status Command: <b>
        private static readonly byte[] BATTERY_STATUS_COMMAND = new[] { (byte)'b' };
        // Motor Driving Commands: <+/-><000~255><a/b/c>
        private readonly byte[] _driveMotorsBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, (byte)'a', 0x00, 0x00, 0x00, 0x00, (byte)'b', 0x00, 0x00, 0x00, 0x00, (byte)'c' };

        private readonly int[] _outputValues = new int[3];
        private readonly int[] _lastOutputValues = new int[3];
        private readonly object _outputLock = new object();

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _writeCharacteristic;
        private IGattCharacteristic _notifyCharacteristic;
        private IGattCharacteristic _hardwareRevisionCharacteristic;
        private IGattCharacteristic _firmwareRevisionCharacteristic;

        public CircuitCubeDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.CircuitCubes;
        public override int NumberOfChannels => 3;

        public override string BatteryVoltageSign => "V";
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            // Per channel range of 0 to 255
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

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _writeCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_WRITE);

            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_FIRMWARE_REVISION);
            _hardwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_HARDWARE_REVISION);

            _notifyCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_NOTIFY);
            if (_notifyCharacteristic != null)
            {
                await _bleDevice?.EnableNotificationAsync(_notifyCharacteristic, token);
            }

            return _writeCharacteristic != null && _firmwareRevisionCharacteristic != null && _hardwareRevisionCharacteristic != null;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != _notifyCharacteristic.Uuid || data.Length <= 1)
                return;

            var bateryVoltage = data.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(bateryVoltage))
            {
                BatteryVoltage = bateryVoltage;
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
                    _lastOutputValues[0] = 1;
                    _lastOutputValues[1] = 1;
                    _lastOutputValues[2] = 1;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                int[] values = new int[NumberOfChannels];
                int sendAttemptsLeft;

                while (!token.IsCancellationRequested)
                {
                    lock (_outputLock)
                    {
                        _outputValues.CopyTo(values, 0);

                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }

                    if (!values.SequenceEqual(_lastOutputValues) || sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(values, token).ConfigureAwait(false))
                        {
                            values.CopyTo(_lastOutputValues, 0);

                            lock (_outputLock)
                            {
                                _sendAttemptsLeft = 0;
                            }
                        }
                        await Task.Delay(5, token).ConfigureAwait(false);
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

        private Task<bool> SendOutputValuesAsync(int[] values, CancellationToken token)
        {
            if (values.All(v => v == 0))
            {
                return SendStopCommandAsync(token);
            }

            return SendDriveCommandAsync(values, token);
        }

        private async Task<bool> SendDriveCommandAsync(int[] values, CancellationToken token)
        {
            try
            {
                // fill all channels (according to doc, it's possible to skip 0, but sometimes it did not stop such channel) 
                int idx = 0;
                foreach (var value in values)
                {
                    // per channel commad: -077c
                    var commendBytes = Encoding.ASCII.GetBytes(value.ToString("+000;-000"));
                    commendBytes.CopyTo(_driveMotorsBuffer, idx);
                    idx += 5;
                }
                return await _bleDevice?.WriteAsync(_writeCharacteristic, _driveMotorsBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendStopCommandAsync(CancellationToken token)
        {
            try
            {
                return await _bleDevice?.WriteAsync(_writeCharacteristic, TURN_OFF_ALL_COMMAND, token);
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

            var hardwareData = await _bleDevice?.ReadAsync(_hardwareRevisionCharacteristic, token);
            var hardwareRevision = hardwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(hardwareRevision))
            {
                HardwareVersion = hardwareRevision;
            }

            await _bleDevice?.WriteAsync(_writeCharacteristic, BATTERY_STATUS_COMMAND, token);
        }
    }
}