using BrickController2.CreationManagement;
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
        public const int NUMBER_OF_PU_PORTS = 4;

        private static readonly Guid SERVICE_UUID = new Guid("500592d1-74fb-4481-88b3-9919b1676e93");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("50052901-74fb-4481-88b3-9919b1676e93");

        private static readonly Guid SERVICE_UUID_DEVICE_INFORMATION = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_MODEL_NUMBER = new Guid("00002a24-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_FIRMWARE_REVISION = new Guid("00002a26-0000-1000-8000-00805f9b34fb");

        private static readonly TimeSpan VoltageMeasurementTimeout = TimeSpan.FromSeconds(5);

        private readonly byte[] _sendOutputBuffer = new byte[] { 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private readonly sbyte[] _outputValues = new sbyte[6];
        private readonly sbyte[] _lastOutputValues = new sbyte[6];
        private readonly object _outputLock = new object();

        private readonly ChannelOutputType[] _channelOutputTypes = new ChannelOutputType[NUMBER_OF_PU_PORTS];
        private readonly int[] _maxServoAngles = new int[NUMBER_OF_PU_PORTS];
        private readonly int[] _servoBaseAngles = new int[NUMBER_OF_PU_PORTS];
        private readonly int[] _stepperAngles = new int[NUMBER_OF_PU_PORTS];

        private readonly int[] _currentStepperAngles = new int[NUMBER_OF_PU_PORTS];

        private readonly short[] _absolutePositions = new short[NUMBER_OF_PU_PORTS];
        private readonly int[] _relativePositions = new int[NUMBER_OF_PU_PORTS];

        private DateTime _positionUpdateTime;

        private readonly object _positionLock = new object();

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

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            lock (_outputLock)
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    _outputValues[channel] = 0;
                    _lastOutputValues[channel] = 0;
                }
            }
            lock (_positionLock)
            {
                // process only PU ports
                for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)                
                {
                    var channelConfig = channelConfigurations.FirstOrDefault(c => c.Channel == channel);

                    _channelOutputTypes[channel] = channelConfig.ChannelOutputType;
                    _absolutePositions[channel] = 0;
                    _relativePositions[channel] = 0;
                    _currentStepperAngles[channel] = 0;

                    _maxServoAngles[channel] = channelConfig.ChannelOutputType == ChannelOutputType.ServoMotor ? channelConfig.MaxServoAngle : 0;
                    _servoBaseAngles[channel] = channelConfig.ChannelOutputType == ChannelOutputType.ServoMotor ? channelConfig.ServoBaseAngle : 0;
                    _stepperAngles[channel] = channelConfig.ChannelOutputType == ChannelOutputType.StepperMotor ? channelConfig.StepperAngle : 0;
                }
                _positionUpdateTime = DateTime.MinValue;
            }
            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (sbyte)(value * 127);

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

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_DEVICE_INFORMATION);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_FIRMWARE_REVISION);
            _modelNumberCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_MODEL_NUMBER);

            return Task.FromResult(_characteristic != null && _firmwareRevisionCharacteristic != null && _modelNumberCharacteristic != null);
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != _characteristic.Uuid || data.Length < 54 || data[0] != 0x01)
            {
                return;
            }

            // Byte 1: Status flags - Bits 3-4 Battery level status (0 - empty, motors disabled; 1 - low; 2 - medium; 3 - full) 

            // do some change filtering as data are comming at 20Hz frequency
            if (GetVoltage(data, out float batteryVoltage))
            {
                BatteryVoltage = $"{batteryVoltage:F2}";
            }

            // Byte 22 - 53: PoweredUp motor data structure(4x)
            //  - Motor type(unsigned 8 - bit)
            //  - Velocity(signed 8 - bit)
            //  - Absolute position(unsigned 16 - bit)
            //  - Position(unsigned 32 - bit)
            lock (_positionLock)
            {
                int baseOffset = 22;
                for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)
                {
                    _absolutePositions[channel] = data.GetInt16(baseOffset + 2);
                    _relativePositions[channel] = data.GetInt32(baseOffset + 4);
#if DEBUG
                    if (data[baseOffset] != 0)
                        System.Diagnostics.Debug.Write($"[{channel}: Type:{data[baseOffset]}, Velocity:{(sbyte)data[baseOffset + 1]}, Abs:{_absolutePositions[channel]}, Pos:{_relativePositions[channel]}]");
#endif
                    baseOffset += 8;
                }
                _positionUpdateTime = DateTime.Now;

#if DEBUG
                System.Diagnostics.Debug.WriteLine("");
#endif
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

                // Configure the function on the target PU port. 
                var poweredUpCfgBuffer = new byte[] { 0x50, 0x00, 0x00, 0x00, 0x00 };
                for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)
                {
                    poweredUpCfgBuffer[1 + channel] = _channelOutputTypes[channel] switch
                    {
                        ChannelOutputType.ServoMotor => 0x16,   // PU absolute position servo
                        ChannelOutputType.StepperMotor => 0x15, // PU position servo 
                        _ => 0x10,                              // PU simple PWM (default) 
                    };
                }
                await _bleDevice?.WriteAsync(_characteristic, poweredUpCfgBuffer, token);
                await Task.Delay(10, token);

                // once configured, enable notification
                await _bleDevice?.EnableNotificationAsync(_characteristic, token);

                // get initial position of a stepper if any
                var baseline = DateTime.Now;
                int attempsLeft = _channelOutputTypes.Any(t => t == ChannelOutputType.StepperMotor) ? MAX_SEND_ATTEMPTS : 0;

                while (attempsLeft-- > 0)
                {
                    lock (_positionLock)
                    {
                        if (_positionUpdateTime >= baseline)
                        {
                            // store current position required for stepper(s)
                            _relativePositions.CopyTo(_currentStepperAngles, 0);
                            break;
                        }
                    }
                    await Task.Delay(20, token);
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Motor cfg:: [{poweredUpCfgBuffer[1]},{poweredUpCfgBuffer[2]},{poweredUpCfgBuffer[3]},{poweredUpCfgBuffer[4]}]");
                System.Diagnostics.Debug.WriteLine($"Base stepper angles: [{_currentStepperAngles[0]},{_currentStepperAngles[1]},{_currentStepperAngles[2]},{_currentStepperAngles[3]}]");
#endif
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
                    sbyte v0, v1, v2, v3, v4, v5;
                    int sendAttemptsLeft;

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
                        if (await SendOutputValuesAsync(new[] { v0, v1, v2, v3 }, v4, v5, token).ConfigureAwait(false))
                        {
                            _lastOutputValues[0] = v0;
                            _lastOutputValues[1] = v1;
                            _lastOutputValues[2] = v2;
                            _lastOutputValues[3] = v3;
                            _lastOutputValues[4] = v4;
                            _lastOutputValues[5] = v5;
                            // reset attemps due to success
                            lock (_outputLock)
                            {
                                _sendAttemptsLeft = 0;
                            }
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

        private async Task<bool> SendOutputValuesAsync(sbyte[] poweredUpValues, sbyte v4, sbyte v5, CancellationToken token)
        {
            try
            {
                // 1 - 16 4x motor reference for ports 1 - 4(signed 32 - bit value for each motor output),
                //           function depends on the PU port state(simple PWM, speed or position servo)
                foreach (var port in poweredUpValues.Select((Value, Index) => (Value, Index)))
                {
                    var channelValue = _channelOutputTypes[port.Index] switch
                    {
                        // angle of servo motor is adjusted according to channel value from range of [-127 .. +127]
                        ChannelOutputType.ServoMotor => _servoBaseAngles[port.Index] + port.Value * _maxServoAngles[port.Index] / 127,
                        // stepper angle is added only if channel value is -127 or +127
                        ChannelOutputType.StepperMotor => _currentStepperAngles[port.Index] += port.Value / 127 * _stepperAngles[port.Index],

                        _ => port.Value
                    };

                    _sendOutputBuffer.SetInt32(1 + 4 * port.Index, channelValue);
                }

                // 17 - 18 2x motor reference for ports 5 - 6(same as bytes 1 - 6 of command 0x30)
                _sendOutputBuffer[17] = (byte)v4;
                _sendOutputBuffer[18] = (byte)v5;

                return await _bleDevice?.WriteAsync(_characteristic, _sendOutputBuffer, token);
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
