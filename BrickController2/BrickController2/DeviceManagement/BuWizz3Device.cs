using BrickController2.CreationManagement;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace BrickController2.DeviceManagement
{
    internal class BuWizz3Device : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;
        private const int NUMBER_OF_PU_PORTS = 4;

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

        private readonly int[] _servoBiasAngles = new int[NUMBER_OF_PU_PORTS];
        private readonly int[] _currentStepperAngles = new int[NUMBER_OF_PU_PORTS];

        private readonly short[] _absolutePositions = new short[NUMBER_OF_PU_PORTS];
        private readonly int[] _relativePositions = new int[NUMBER_OF_PU_PORTS];
        private readonly object _positionLock = new object();
        private readonly ManualResetEventSlim _characteristicNotificationResetEvent = new ManualResetEventSlim();

        private int _sendAttemptsLeft;

        private DateTime _batteryMeasurementTimestamp;
        private byte _batteryVoltageRaw;

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

        public override bool CanChangeOutputType(int channel) => channel < NUMBER_OF_PU_PORTS;

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
            }

            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var sbyteValue = (sbyte)(value * 127);

            lock (_outputLock)
            {
                if (_outputValues[channel] != sbyteValue)
                {
                    _outputValues[channel] = sbyteValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        public override bool CanBePowerSource => false;

        public override bool CanResetOutput(int channel) => channel < NUMBER_OF_PU_PORTS;

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            if (channel >= NUMBER_OF_PU_PORTS)
            {
                return;
            }

            await ResetServoAsync(channel, Convert.ToInt32(value * 180), token).ConfigureAwait(false);
        }

        public override bool CanAutoCalibrateOutput(int channel) => channel < NUMBER_OF_PU_PORTS;

        public override async Task<(bool Success, float BaseServoAngle)> AutoCalibrateOutputAsync(int channel, CancellationToken token)
        {
            CheckChannel(channel);

            if (channel >= NUMBER_OF_PU_PORTS)
            {
                return (false, 0);
            }

            return await AutoCalibrateServoAsync(channel, token).ConfigureAwait(false);
        }

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
            if (TryGetVoltage(data, out float batteryVoltage))
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

                    baseOffset += 8;
                }
            }

            _characteristicNotificationResetEvent.Set();
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    await ReadDeviceInfo(token).ConfigureAwait(false);
                }

                var result = true;

                result = result && await _bleDevice.EnableNotificationAsync(_characteristic, token).ConfigureAwait(false);
                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);

                result = result && await ResetMotorRampUpDownAsync(token).ConfigureAwait(false);
                result = result && await SetServoReferencesAsync(new[] { 0, 0, 0, 0 }, token).ConfigureAwait(false);

                // Need to set the modes a couple of times to take effect
                for (int i = 0; i < 4; i++)
                {
                    result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);

                    result = result && await SetPuPortModesAsync(token).ConfigureAwait(false);

                    var servoRefs = new int[NUMBER_OF_PU_PORTS];
                    for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)
                    {
                        servoRefs[channel] = CalculateServoReference(_absolutePositions[channel], _relativePositions[channel], _servoBaseAngles[channel]);

                        if (_channelOutputTypes[channel] == ChannelOutputType.ServoMotor)
                        {
                            result = result && await SetDefaultPidParametersAsync(channel, true, token).ConfigureAwait(false);
                        }
                    }

                    result = result && await SetServoReferencesAsync(servoRefs, token).ConfigureAwait(false);
                    await Task.Delay(200);
                }

                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);
                _relativePositions.CopyTo(_servoBiasAngles, 0);
                _relativePositions.CopyTo(_currentStepperAngles, 0);

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                lock (_outputLock)
                lock (_positionLock)
                {
                    for (int channel = 0; channel < NumberOfChannels; channel++)
                    {
                        _outputValues[channel] = 0;
                        _lastOutputValues[channel] = 1;
                    }

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
                        await Task.Delay(50, token).ConfigureAwait(false);
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
                foreach (var channel in poweredUpValues.Select((Value, Index) => (Value, Index)))
                {
                    var channelValue = _channelOutputTypes[channel.Index] switch
                    {
                        // angle of servo motor is adjusted according to channel value from range of [-127 .. +127]
                        ChannelOutputType.ServoMotor => _servoBiasAngles[channel.Index] + channel.Value * _maxServoAngles[channel.Index] / 127,
                        // keep the previous angle for stepper if there is no change of channel value 
                        ChannelOutputType.StepperMotor when channel.Value == _lastOutputValues[channel.Index] => _currentStepperAngles[channel.Index],
                        // stepper angle is added only if channel value is -127 or +127
                        ChannelOutputType.StepperMotor => _currentStepperAngles[channel.Index] += channel.Value / 127 * _stepperAngles[channel.Index],

                        _ => channel.Value
                    };

                    _sendOutputBuffer.SetInt32(channelValue, 1 + 4 * channel.Index);
                }

                // 17 - 18 2x motor reference for ports 5 - 6(same as bytes 1 - 6 of command 0x30)
                _sendOutputBuffer[17] = (byte)v4;
                _sendOutputBuffer[18] = (byte)v5;

                var result = await _bleDevice.WriteNoResponseAsync(_characteristic, _sendOutputBuffer, token).ConfigureAwait(false);
                await Task.Delay(100, token).ConfigureAwait(false); // this delay is needed not to flood the BW3 internal command queue
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryGetVoltage(byte[] data, out float batteryVoltage)
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
            var firmwareData = await _bleDevice.ReadAsync(_firmwareRevisionCharacteristic, token).ConfigureAwait(false);
            var firmwareVersion = firmwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(firmwareVersion))
            {
                FirmwareVersion = firmwareVersion;
            }

            var modelNumberData = await _bleDevice.ReadAsync(_modelNumberCharacteristic, token).ConfigureAwait(false);
            var modelNumber = modelNumberData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(modelNumber))
            {
                HardwareVersion = modelNumber;
            }
        }

        private async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var result = true;

                result = result && await ResetMotorRampUpDownAsync(token).ConfigureAwait(false);
                result = result && await SetServoReferenceAsync(channel, 0, token).ConfigureAwait(false);

                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);
                var absPosStart = _absolutePositions[channel];
                var relPosStart = _relativePositions[channel];
                var servoReference = CalculateServoReference(absPosStart, relPosStart, baseAngle);

                result = result && await SetPuPortModeAsync(channel, true, token).ConfigureAwait(false);
                result = result && await SetDefaultPidParametersAsync(channel, true, token).ConfigureAwait(false);

                result = await SetServoReferenceAsync(channel, servoReference, token).ConfigureAwait(false);
                await Task.Delay(500).ConfigureAwait(false);

                result = result && await SetPuPortModeAsync(channel, false, token).ConfigureAwait(false);
                result = result && await SetSpeedAsync(channel, 0, token).ConfigureAwait(false);

                return result;
            }
            catch
            {
                return false;
            }
        }

        private int CalculateServoReference(short absPosStart, int relPosStart, int baseAngle)
        {
            var posStart = absPosStart - relPosStart;
            var servoReference = baseAngle - posStart;

            var absDiff = Math.Abs(baseAngle - absPosStart);

            if (180 <= absDiff)
            {
                servoReference = absPosStart < 0 ?
                    servoReference - 360 :
                    servoReference + 360;
            }

            return servoReference;
        }

        private async Task<(bool, float)> AutoCalibrateServoAsync(int channel, CancellationToken token)
        {
            try
            {
                var result = true;

                result = result && await SetServoReferenceAsync(channel, 0, token).ConfigureAwait(false);

                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);
                var absPosStart = _absolutePositions[channel];
                var relPosStart = _relativePositions[channel];

                result = result && await SetPuPortModeAsync(channel, false, token).ConfigureAwait(false);
                result = result && await SetSpeedAsync(channel, 0x33, token).ConfigureAwait(false);
                await Task.Delay(1000);
                result = result && await SetSpeedAsync(channel, 0, token).ConfigureAwait(false);
                await Task.Delay(100);

                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);
                var absPos1 = _absolutePositions[channel];
                var relPos1 = _relativePositions[channel];

                result = result && await SetSpeedAsync(channel, -0x33, token).ConfigureAwait(false);
                await Task.Delay(1200).ConfigureAwait(false);
                result = result && await SetSpeedAsync(channel, 0, token).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);

                result = result && await WaitForNextCharacteristicNotificationAsync(token).ConfigureAwait(false);
                var absPos2 = _absolutePositions[channel];
                var relPos2 = _relativePositions[channel];

                result = result && await SetPuPortModeAsync(channel, true, token).ConfigureAwait(false);
                result = result && await SetDefaultPidParametersAsync(channel, true, token).ConfigureAwait(false);

                var absPos2Corrected = (absPos2 <= absPos1) ? absPos2 : absPos2 - 360;
                var absPosMid = RoundAngleToNearest90((absPos1 + absPos2Corrected) / 2);

                var servoReference = CalculateServoReference(absPosStart, relPosStart, absPosMid);

                result = result && await SetServoReferenceAsync(channel, servoReference, token).ConfigureAwait(false);
                await Task.Delay(500).ConfigureAwait(false);

                result = result && await SetPuPortModeAsync(channel, false, token).ConfigureAwait(false);
                result = result && await SetSpeedAsync(channel, 0, token).ConfigureAwait(false);

                return (result, absPosMid / 180f);
            }
            catch
            {
                return (false, 0F);
            }
        }

        private async Task<bool> SetServoReferenceAsync(int channel, int value, CancellationToken token)
        {
            var buffer = new byte[] { 0x52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            buffer.SetInt32(value, 1 + channel * 4);
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SetServoReferencesAsync(int[] refValues, CancellationToken token)
        {
            var buffer = new byte[] { 0x52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)
            {
                buffer.SetInt32(refValues[channel], 1 + channel * 4);
            }
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SetPuPortModesAsync(CancellationToken token)
        {
            var buffer = new byte[] { 0x50, 0x10, 0x10, 0x10, 0x10 };
            for (int channel = 0; channel < NUMBER_OF_PU_PORTS; channel++)
            {
                buffer[1 + channel] = _channelOutputTypes[channel] switch
                {
                    ChannelOutputType.ServoMotor => 0x15,
                    ChannelOutputType.StepperMotor => 0x16, // Not sure about this
                    _ => 0x10
                };
            }
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SetPuPortModeAsync(int channel, bool isServo, CancellationToken token)
        {
            var buffer = new byte[] { 0x50, 0x10, 0x10, 0x10, 0x10 };
            buffer[1 + channel] = isServo ? (byte)0x15 : (byte)0x10;
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SetSpeedAsync(int channel, int value, CancellationToken token)
        {
            var buffer = new byte[] { 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            buffer.SetInt32(value, 1 + channel * 4);
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> ResetMotorRampUpDownAsync(CancellationToken token)
        {
            var buffer = new byte[] { 0x33, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(50, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> SetDefaultPidParametersAsync(int channel, bool isServo, CancellationToken token)
        {
            var buffer = new byte[38];

            buffer[0] = 0x53;
            buffer[1] = (byte)channel;
            buffer.SetFloat(0f, 2); // outLP
            buffer.SetFloat(0f, 6); // D_LP
            buffer.SetFloat(0.6f, 10); // speed_LP
            buffer.SetFloat(0.5f, 14); // Kp
            buffer.SetFloat(0.01f, 18); // Ki
            buffer.SetFloat(-1f, 22); // Kd
            buffer.SetFloat(20f, 26); // Liml
            buffer.SetFloat(50f, 30); // Reference rate limit
            buffer[34] = 127; // limOut
            buffer[35] = 5; // DeadbandOut
            buffer[36] = 10; // DeadbandOutBoost
            buffer[37] = isServo ? (byte)0x15 : (byte)0x10; // valid mode (equal to port mode selected)

            var result = await _bleDevice.WriteAsync(_characteristic, buffer, token).ConfigureAwait(false);
            await Task.Delay(100, token).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> WaitForNextCharacteristicNotificationAsync(CancellationToken token)
        {
            _characteristicNotificationResetEvent.Reset();
            return await _characteristicNotificationResetEvent.WaitAsync(token).ConfigureAwait(false);
        }

        private int NormalizeAngle(int angle)
        {
            if (angle >= 180)
            {
                return angle - (360 * ((angle + 180) / 360));
            }
            else if (angle < -180)
            {
                return angle + (360 * ((180 - angle) / 360));
            }

            return angle;
        }

        private int RoundAngleToNearest90(int angle)
        {
            angle = NormalizeAngle(angle);
            if (angle < -135) return -180;
            if (angle < -45) return -90;
            if (angle < 45) return 0;
            if (angle < 135) return 90;
            return -180;
        }
    }
}
