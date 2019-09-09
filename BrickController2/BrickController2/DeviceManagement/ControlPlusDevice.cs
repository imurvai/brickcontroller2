using BrickController2.CreationManagement;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class ControlPlusDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private static readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        private readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };

        private readonly TimeSpan _sendDelay = TimeSpan.FromMilliseconds(40);

        private readonly int[] _outputValues;
        private readonly int[] _lastOutputValues;
        private readonly int[] _maxServoAngles;
        private readonly int[] _servoBaseAngles;
        private readonly int[] _absolutePositions;
        private readonly int[] _relativePositions;

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            _outputValues = new int[NumberOfChannels];
            _lastOutputValues = new int[NumberOfChannels];
            _maxServoAngles = new int[NumberOfChannels];
            _servoBaseAngles = new int[NumberOfChannels];
            _absolutePositions = new int[NumberOfChannels];
            _relativePositions = new int[NumberOfChannels];
        }

        protected override bool AutoConnectOnFirstConnect => true;

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            CancellationToken token)
        {
            for (int c = 0; c < NumberOfChannels; c++)
            {
                _outputValues[c] = 0;
                _lastOutputValues[c] = 0;
                _maxServoAngles[c] = -1;
                _servoBaseAngles[c] = 0;
                _absolutePositions[c] = 0;
                _relativePositions[c] = 0;
            }

            foreach (var channelConfig in channelConfigurations)
            {
                if (channelConfig.ChannelOutputType == ChannelOutputType.ServoMotor)
                {
                    _maxServoAngles[channelConfig.Channel] = channelConfig.MaxServoAngle;
                    _servoBaseAngles[channelConfig.Channel] = channelConfig.ServoBaseAngle;
                }
            }

            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(100 * value);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        public override void ResetOutput(int channel, float value)
        {
            CheckChannel(channel);

            // TODO: finish
        }

        protected override bool ValidateServices(IEnumerable<IGattService> services)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            if (_characteristic != null)
            {
                _bleDevice.EnableNotification(_characteristic);
                return true;
            }

            return false;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != CHARACTERISTIC_UUID || data.Length < 4)
            {
                return;
            }

            var text = BitConverter.ToString(data);

            var messageCode = data[2];
            var portId = data[3];

            if (portId < 0 || portId >= NumberOfChannels)
            {
                return;
            }

            switch (messageCode)
            {
                case 0x46: // Port value information
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
                        var relPosBuffer = BitConverter.IsLittleEndian ?
                            new byte[] { data[dataIndex + 0], data[dataIndex + 1], data[dataIndex + 2], data[dataIndex + 3] } :
                            new byte[] { data[dataIndex + 3], data[dataIndex + 2], data[dataIndex + 1], data[dataIndex + 0] };

                        var relPosition = BitConverter.ToInt32(relPosBuffer, 0);
                        _relativePositions[portId] = relPosition;
                    }

                    break;
            }
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    _outputValues[channel] = 0;
                    _lastOutputValues[channel] = 1;
                }

                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    if (_sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(token))
                        {
                            var isAllZero = true;
                            for (var channel = 0; channel < NumberOfChannels; channel++)
                            {
                                isAllZero = isAllZero && _outputValues[channel] == 0;
                            }

                            if (isAllZero)
                            {
                                _sendAttemptsLeft--;
                            }
                            else
                            {
                                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
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
                // Do nothing here, just exit
            }
        }

        protected override async Task<bool> AfterConnectSetupAsync(CancellationToken token)
        {
            try
            {
                // Wait until ports finish communicating with the hub
                await Task.Delay(1000, token);

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    if (_maxServoAngles[channel] >= 0)
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
                    var outputValue = _outputValues[channel];
                    var maxServoAngle = _maxServoAngles[channel];

                    if (maxServoAngle < 0)
                    {
                        result = result && await SendOutputValueAsync(channel, outputValue, token);
                    }
                    else
                    {
                        result = result && await SendServoOutputValueAsync(channel, outputValue, maxServoAngle, token);
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueAsync(int channel, int value, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    _sendBuffer[3] = (byte)channel;
                    _sendBuffer[7] = (byte)(value < 0 ? (255 + value) : value);

                    if (_bleDevice?.WriteNoResponse(_characteristic, _sendBuffer) == true)
                    {
                        _lastOutputValues[channel] = value;

                        await Task.Delay(_sendDelay, token);
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

                    if (_bleDevice?.WriteNoResponse(_characteristic, _virtualPortSendBuffer) == true)
                    {
                        _lastOutputValues[channel1] = value1;
                        _lastOutputValues[channel2] = value2;

                        await Task.Delay(_sendDelay, token);
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

        private async Task<bool> SendServoOutputValueAsync(int channel, int value, int maxServoAngle, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    var servoValue = maxServoAngle * value / 100;
                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);
                    _servoSendBuffer[10] = CalculateServoSpeed(_relativePositions[channel], servoValue);

                    if (_bleDevice?.WriteNoResponse(_characteristic, _servoSendBuffer) == true)
                    {
                        _lastOutputValues[channel] = value;

                        await Task.Delay(_sendDelay, token);
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

                var absAngle = _absolutePositions[channel];
                var resetToAngle = absAngle - baseAngle;

                if (resetToAngle < -180) resetToAngle += 360;
                else if (resetToAngle > 179) resetToAngle -= 360;
                
                var ra0 = (byte)(resetToAngle & 0xff);
                var ra1 = (byte)((resetToAngle >> 8) & 0xff);
                var ra2 = (byte)((resetToAngle >> 16) & 0xff);
                var ra3 = (byte)((resetToAngle >> 24) & 0xff);

                var resetToZeroBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, 0x00, 0x00, 0x00, 0x00 };
                var stopBuffer = new byte[] { 0x08, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x00, 0x00 };
                var turnToZeroBuffer = new byte[] { 0x0e, 0x00, 0x81, (byte)channel, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 30, 0x64, 0x7e, 0x00 };
                var resetToBaseBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, ra0, ra1, ra2, ra3 };

                var result = true;

                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer, token);
                result = result && await _bleDevice.WriteAsync(_characteristic, resetToZeroBuffer, token);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer, token);
                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer, token);
                result = result && await _bleDevice.WriteAsync(_characteristic, resetToBaseBuffer, token);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer, token);

                return result;
            }
            catch
            {
                return false;
            }
        }

        private byte CalculateServoSpeed(int currentAngle, int targetAngle)
        {
            var diff = Math.Abs(currentAngle - targetAngle);
            var result = (byte)Math.Max(5, Math.Min(100, diff));
            return result;
        }
    }
}
