using BrickController2.CreationManagement;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal abstract class ControlPlusDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private static readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private static readonly TimeSpan SEND_DELAY = TimeSpan.FromMilliseconds(60);
        private static readonly TimeSpan POSITION_EXPIRATION = TimeSpan.FromMilliseconds(200);

        private readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        private readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _stepperSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0b, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };

        private readonly int[] _outputValues;
        private readonly int[] _lastOutputValues;
        private readonly int[] _sendAttemptsLeft;
        private readonly object _outputLock = new object();

        private readonly ChannelOutputType[] _channelOutputTypes;
        private readonly int[] _maxServoAngles;
        private readonly int[] _servoBaseAngles;
        private readonly int[] _stepperAngles;

        private readonly int[] _absolutePositions;
        private readonly int[] _relativePositions;
        private readonly bool[] _positionsUpdated;
        private readonly DateTime[] _positionUpdateTimes;
        private readonly object _positionLock = new object();

        private IGattCharacteristic _characteristic;

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            _outputValues = new int[NumberOfChannels];
            _lastOutputValues = new int[NumberOfChannels];
            _sendAttemptsLeft = new int[NumberOfChannels];

            _channelOutputTypes = new ChannelOutputType[NumberOfChannels];
            _maxServoAngles = new int[NumberOfChannels];
            _servoBaseAngles = new int[NumberOfChannels];
            _stepperAngles = new int[NumberOfChannels];

            _absolutePositions = new int[NumberOfChannels];
            _relativePositions = new int[NumberOfChannels];
            _positionsUpdated = new bool[NumberOfChannels];
            _positionUpdateTimes = new DateTime[NumberOfChannels];
        }

        public override string BatteryVoltageSign => "%";

        public override bool CanChangeOutputType(int channel) => true;

        protected override bool AutoConnectOnFirstConnect => true;

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            lock (_outputLock)
                lock (_positionLock)
                {
                    for (int c = 0; c < NumberOfChannels; c++)
                    {
                        _outputValues[c] = 0;
                        _lastOutputValues[c] = 0;

                        _channelOutputTypes[c] = ChannelOutputType.NormalMotor;
                        _maxServoAngles[c] = 0;
                        _servoBaseAngles[c] = 0;
                        _stepperAngles[c] = 0;

                        _absolutePositions[c] = 0;
                        _relativePositions[c] = 0;
                        _positionsUpdated[c] = false;
                        _positionUpdateTimes[c] = DateTime.MinValue;
                    }
                }

            foreach (var channelConfig in channelConfigurations)
            {
                _channelOutputTypes[channelConfig.Channel] = channelConfig.ChannelOutputType;

                switch (channelConfig.ChannelOutputType)
                {
                    case ChannelOutputType.NormalMotor:
                        break;

                    case ChannelOutputType.ServoMotor:
                        _maxServoAngles[channelConfig.Channel] = channelConfig.MaxServoAngle;
                        _servoBaseAngles[channelConfig.Channel] = channelConfig.ServoBaseAngle;
                        break;

                    case ChannelOutputType.StepperMotor:
                        _stepperAngles[channelConfig.Channel] = channelConfig.StepperAngle;
                        break;
                }
            }

            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(100 * value);

            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                }
            }
        }

        public override bool CanResetOutput(int channel) => true;

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(300, token);
            await ResetServoAsync(channel, Convert.ToInt32(value * 180), token);
        }

        public override bool CanAutoCalibrateOutput(int channel) => true;

        public override bool CanChangeMaxServoAngle(int channel) => true;

        public override async Task<(bool Success, float BaseServoAngle)> AutoCalibrateOutputAsync(int channel, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(TimeSpan.FromMilliseconds(300), token);
            return await AutoCalibrateServoAsync(channel, token);
        }

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            if (_characteristic != null)
            {
                return await _bleDevice?.EnableNotificationAsync(_characteristic, token);
            }

            return false;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != CHARACTERISTIC_UUID || data.Length < 4)
            {
                return;
            }

            var messageCode = data[2];

            switch (messageCode)
            {
                case 0x01: // Hub properties
                    ProcessHubPropertyData(data);
                    break;

                case 0x02: // Hub actions
                    DumpData("Hub actions", data);
                    break;

                case 0x03: // Hub alerts
                    DumpData("Hub alerts", data);
                    break;

                case 0x04: // Hub attached I/O
                    DumpData("Hub attached I/O", data);
                    break;

                case 0x05: // Generic error messages
                    DumpData("Generic error messages", data);
                    break;

                case 0x08: // HW network commands
                    DumpData("HW network commands", data);
                    break;

                case 0x13: // FW lock status
                    DumpData("FW lock status", data);
                    break;

                case 0x43: // Port information
                    DumpData("Port information", data);
                    break;

                case 0x44: // Port mode information
                    DumpData("Port mode information", data);
                    break;

                case 0x45: // Port value (single mode)
                    DumpData("Port value (single)", data);
                    break;

                case 0x46: // Port value (combined mode)
                    lock (_positionLock)
                    {
                        var portId = data[3];
                        var modeMask = data[5];
                        var dataIndex = 6;

                        if ((modeMask & 0x01) != 0)
                        {
                            var absPosBuffer = BitConverter.IsLittleEndian ?
                                new byte[] { data[dataIndex + 0], data[dataIndex + 1] } :
                                new byte[] { data[dataIndex + 1], data[dataIndex + 0] };

                            var absPosition = BitConverter.ToInt16(absPosBuffer, 0);
                            _absolutePositions[portId] = absPosition;

                            dataIndex += 2;
                        }

                        if ((modeMask & 0x02) != 0)
                        {
                            // TODO: Read the post value format response and determine the value length accordingly
                            if ((dataIndex + 3) < data.Length)
                            {
                                var relPosBuffer = BitConverter.IsLittleEndian ?
                                    new byte[] { data[dataIndex + 0], data[dataIndex + 1], data[dataIndex + 2], data[dataIndex + 3] } :
                                    new byte[] { data[dataIndex + 3], data[dataIndex + 2], data[dataIndex + 1], data[dataIndex + 0] };

                                var relPosition = BitConverter.ToInt32(relPosBuffer, 0);
                                _relativePositions[portId] = relPosition;
                            }
                            else if ((dataIndex + 1) < data.Length)
                            {
                                var relPosBuffer = BitConverter.IsLittleEndian ?
                                    new byte[] { data[dataIndex + 0], data[dataIndex + 1] } :
                                    new byte[] { data[dataIndex + 1], data[dataIndex + 0] };

                                var relPosition = BitConverter.ToInt16(relPosBuffer, 0);
                                _relativePositions[portId] = relPosition;
                            }
                            else
                            {
                                _relativePositions[portId] = data[dataIndex];
                            }

                            _positionsUpdated[portId] = true;
                            _positionUpdateTimes[portId] = DateTime.Now;
                        }
                    }

                    break;

                case 0x47: // Port input format (Single mode)
                    DumpData("Port input format (single)", data);
                    break;

                case 0x48: // Port input format (Combined mode)
                    DumpData("Port input format (combined)", data);
                    break;

                case 0x82: // Port output command feedback
                    break;
            }
        }

        private void DumpData(string header, byte[] data)
        {
            //var s = BitConverter.ToString(data);
            //Console.WriteLine(header + " - " + s);
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
                            _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                            _positionsUpdated[channel] = false;
                            _positionUpdateTimes[channel] = DateTime.MinValue;
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

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                // Wait until ports finish communicating with the hub
                await Task.Delay(1000, token);

                if (requestDeviceInformation)
                {
                    await RequestHubPropertiesAsync(token);
                }

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    if (_channelOutputTypes[channel] == ChannelOutputType.ServoMotor)
                    {
                        await SetupChannelForPortInformationAsync(channel, token);
                        await Task.Delay(300, token);
                        await ResetServoAsync(channel, _servoBaseAngles[channel], token);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                var result = true;

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    switch (_channelOutputTypes[channel])
                    {
                        case ChannelOutputType.NormalMotor:
                            result = result && await SendOutputValueAsync(channel, token);
                            break;

                        case ChannelOutputType.ServoMotor:
                            var maxServoAngle = _maxServoAngles[channel];
                            result = result && await SendServoOutputValueAsync(channel, token);
                            break;

                        case ChannelOutputType.StepperMotor:
                            result = result && await SendStepperOutputValueAsync(channel, token);
                            break;
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                if (v != _lastOutputValues[channel] || sendAttemptsLeft > 0)
                {
                    _sendBuffer[3] = (byte)channel;
                    _sendBuffer[7] = (byte)(v < 0 ? (255 + v) : v);

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _sendBuffer, token))
                    {
                        _lastOutputValues[channel] = v;
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueVirtualAsync(int virtualChannel, int channel1, int channel2, int value1, int value2, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel1] != value1 || _lastOutputValues[channel2] != value2)
                {
                    _virtualPortSendBuffer[3] = (byte)virtualChannel;
                    _virtualPortSendBuffer[6] = (byte)(value1 < 0 ? (255 + value1) : value1);
                    _virtualPortSendBuffer[7] = (byte)(value2 < 0 ? (255 + value2) : value2);

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _virtualPortSendBuffer, token))
                    {
                        _lastOutputValues[channel1] = value1;
                        _lastOutputValues[channel2] = value2;

                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendServoOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                if (v != _lastOutputValues[channel] || sendAttemptsLeft > 0)
                {
                    var servoValue = _maxServoAngles[channel] * v / 100;
                    var servoSpeed = CalculateServoSpeed(channel, servoValue);

                    if (servoSpeed == 0)
                    {
                        return true;
                    }

                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);
                    _servoSendBuffer[10] = (byte)servoSpeed;

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _servoSendBuffer, token))
                    {
                        _lastOutputValues[channel] = v;
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendStepperOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                var stepperAngle = _stepperAngles[channel];
                _stepperSendBuffer[3] = (byte)channel;
                _stepperSendBuffer[6] = (byte)(stepperAngle & 0xff);
                _stepperSendBuffer[7] = (byte)((stepperAngle >> 8) & 0xff);
                _stepperSendBuffer[8] = (byte)((stepperAngle >> 16) & 0xff);
                _stepperSendBuffer[9] = (byte)((stepperAngle >> 24) & 0xff);
                _stepperSendBuffer[10] = (byte)(v > 0 ? 50 : -50);

                if (v != _lastOutputValues[channel] && Math.Abs(v) == 100)
                {
                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _stepperSendBuffer, token))
                    {
                        _lastOutputValues[channel] = v;
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _lastOutputValues[channel] = v;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                var lockBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x02 };
                var inputFormatForAbsAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x03, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var inputFormatForRelAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x02, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var modeAndDataSetBuffer = new byte[] { 0x08, 0x00, 0x42, (byte)channel, 0x01, 0x00, 0x30, 0x20 };
                var unlockAndEnableBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x03 };

                var result = true;
                result = result && await _bleDevice?.WriteAsync(_characteristic, lockBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, inputFormatForAbsAngleBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, inputFormatForRelAngleBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, modeAndDataSetBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, unlockAndEnableBuffer, token);

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var resetToAngle = NormalizeAngle(_absolutePositions[channel] - baseAngle);

                var result = true;

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(50);
                result = result && await StopAsync(channel, token);
                result = result && await ResetAsync(channel, resetToAngle, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(500);
                result = result && await StopAsync(channel, token);

                var diff = Math.Abs(NormalizeAngle(_absolutePositions[channel] - baseAngle));
                if (diff > 5)
                {
                    // Can't reset to base angle, rebease to current position not to stress the plastic
                    result = result && await ResetAsync(channel, 0, token);
                    result = result && await StopAsync(channel, token);
                    result = result && await TurnAsync(channel, 0, 40, token);
                    await Task.Delay(50);
                    result = result && await StopAsync(channel, token);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(bool, float)> AutoCalibrateServoAsync(int channel, CancellationToken token)
        {
            try
            {
                var result = true;

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 50, token);
                await Task.Delay(600);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500);
                var absPositionAt0 = _absolutePositions[channel];
                result = result && await TurnAsync(channel, -160, 60, token);
                await Task.Delay(600);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500);
                var absPositionAtMin160 = _absolutePositions[channel];
                result = result && await TurnAsync(channel, 160, 60, token);
                await Task.Delay(600);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500);
                var absPositionAt160 = _absolutePositions[channel];

                var midPoint1 = NormalizeAngle((absPositionAtMin160 + absPositionAt160) / 2);
                var midPoint2 = NormalizeAngle(midPoint1 + 180);

                var baseAngle = (Math.Abs(NormalizeAngle(midPoint1 - absPositionAt0)) < Math.Abs(NormalizeAngle(midPoint2 - absPositionAt0))) ?
                    RoundAngleToNearest90(midPoint1) :
                    RoundAngleToNearest90(midPoint2);
                var resetToAngle = NormalizeAngle(_absolutePositions[channel] - baseAngle);

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(50);
                result = result && await StopAsync(channel, token);
                result = result && await ResetAsync(channel, resetToAngle, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(600);
                result = result && await StopAsync(channel, token);

                return (result, baseAngle / 180F);
            }
            catch
            {
                return (false, 0F);
            }
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

        private int CalculateServoSpeed(int channel, int targetAngle)
        {
            lock (_positionLock)
            {
                if (_positionsUpdated[channel])
                {
                    var diffAngle = Math.Abs(_relativePositions[channel] - targetAngle);
                    _positionsUpdated[channel] = false;

                    return Math.Max(20, Math.Min(100, diffAngle));
                }

                var positionUpdateTime = _positionUpdateTimes[channel];
                if (positionUpdateTime == DateTime.MinValue ||
                    POSITION_EXPIRATION < DateTime.Now - positionUpdateTime)
                {
                    // Position update never happened or too old
                    return 50;
                }
            }

            return 0;
        }

        private Task<bool> StopAsync(int channel, CancellationToken token)
        {
            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x08, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x00, 0x00 }, token);
        }

        private Task<bool> TurnAsync(int channel, int angle, int speed, CancellationToken token)
        {
            angle = NormalizeAngle(angle);

            var a0 = (byte)(angle & 0xff);
            var a1 = (byte)((angle >> 8) & 0xff);
            var a2 = (byte)((angle >> 16) & 0xff);
            var a3 = (byte)((angle >> 24) & 0xff);

            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x0e, 0x00, 0x81, (byte)channel, 0x11, 0x0d, a0, a1, a2, a3, (byte)speed, 0x64, 0x7e, 0x00 }, token);
        }

        private Task<bool> ResetAsync(int channel, int angle, CancellationToken token)
        {
            angle = NormalizeAngle(angle);

            var a0 = (byte)(angle & 0xff);
            var a1 = (byte)((angle >> 8) & 0xff);
            var a2 = (byte)((angle >> 16) & 0xff);
            var a3 = (byte)((angle >> 24) & 0xff);

            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, a0, a1, a2, a3 }, token);
        }

        private async Task RequestHubPropertiesAsync(CancellationToken token)
        {
            try
            {
                // Request firmware version
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                await _bleDevice?.WriteAsync(_characteristic, new byte[] { 0x05, 0x00, 0x01, 0x03, 0x05 }, token);
                var data = await _bleDevice?.ReadAsync(_characteristic, token);
                ProcessHubPropertyData(data);

                // Request hardware version
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                await _bleDevice?.WriteAsync(_characteristic, new byte[] { 0x05, 0x00, 0x01, 0x04, 0x05 }, token);
                data = await _bleDevice?.ReadAsync(_characteristic, token);
                ProcessHubPropertyData(data);

                // Request battery voltage
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                await _bleDevice?.WriteAsync(_characteristic, new byte[] { 0x05, 0x00, 0x01, 0x06, 0x05 }, token);
                data = await _bleDevice?.ReadAsync(_characteristic, token);
                ProcessHubPropertyData(data);
            }
            catch { }
        }

        private void ProcessHubPropertyData(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 6)
                {
                    return;
                }

                var dataLength = data[0];
                var messageId = data[2];
                var propertyId = data[3];
                var propertyOperation = data[4];

                if (messageId != 0x01 || propertyOperation != 0x06)
                {
                    // Operation is not 'update'
                    return;
                }

                switch (propertyId)
                {
                    case 0x03: // FW version
                        var firmwareVersion = ProcessVersionNumber(data, 5);
                        if (!string.IsNullOrEmpty(firmwareVersion))
                        {
                            FirmwareVersion = firmwareVersion;
                        }
                        break;

                    case 0x04: // HW version
                        var hardwareVersion = ProcessVersionNumber(data, 5);
                        if (!string.IsNullOrEmpty(hardwareVersion))
                        {
                            HardwareVersion = hardwareVersion;
                        }
                        break;

                    case 0x06: // Battery voltage
                        var voltage = data[5];
                        BatteryVoltage = voltage.ToString("F0");
                        break;
                }
            }
            catch { }
        }

        private string ProcessVersionNumber(byte[] data, int index)
        {
            if (data.Length < index + 4)
            {
                return null;
            }

            var v0 = data[index];
            var v1 = data[index + 1];
            var v2 = data[index + 2];
            var v3 = data[index + 3];

            var major = v3 >> 4;
            var minor = v3 & 0xf;
            var bugfix = ((v2 >> 4) * 10) + (v2 & 0xf);
            var build = ((v1 >> 4) * 1000) + ((v1 & 0xf) * 100) + ((v0 >> 4) * 10) + (v0 & 0xf);

            return $"{major}.{minor}.{bugfix}.{build}";
        }
    }
}
