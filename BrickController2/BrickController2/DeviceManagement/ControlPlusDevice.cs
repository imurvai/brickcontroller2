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
        protected const int MAX_SEND_ATTEMPTS = 4;

        protected static readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        protected static readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        protected readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        protected readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        protected readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };
        protected readonly byte[] _virtualPortSetupBuffer = new byte[] { 6, 0x00, 0x61, 0x01, 0x00, 0x00 };

        private readonly TimeSpan _sendDelay = TimeSpan.FromMilliseconds(40);

        protected readonly int[] _outputValues;
        protected readonly int[] _lastOutputValues;
        protected readonly int[] _maxServoAngles;
        protected readonly int[] _servoBaseAngles;
        protected readonly int[] _absolutePositions;

        protected volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            _outputValues = new int[NumberOfChannels];
            _lastOutputValues = new int[NumberOfChannels];
            _maxServoAngles = new int[NumberOfChannels];
            _servoBaseAngles = new int[NumberOfChannels];
            _absolutePositions = new int[NumberOfChannels];
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

            var messageCode = data[2];
            var portId = data[3];

            switch (messageCode)
            {
                case 0x46: // Port value information
                    if (data.Length >= 8)
                    {
                        var modeMask = data[5];
                        if ((modeMask & 0x01) != 0)
                        {
                            var absPosLowHigh = BitConverter.IsLittleEndian ?
                                new byte[] { data[6], data[7] } :
                                new byte[] { data[7], data[6] };

                            if (portId >= 0 && portId < _absolutePositions.Length)
                            {
                                _absolutePositions[portId] = BitConverter.ToInt16(absPosLowHigh, 0);
                            }
                        }
                    }
                    break;
            }
        }

        //protected async Task<bool> SetupVirtualPortAsync(int channel1, int channel2)
        //{
        //    try
        //    {
        //        _virtualPortSetupBuffer[4] = (byte)channel1;
        //        _virtualPortSetupBuffer[5] = (byte)channel2;

        //        return await _bleDevice?.WriteAsync(_characteristic, _virtualPortSetupBuffer);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        protected async Task<bool> SendOutputValueAsync(int channel, int value)
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

                        await Task.Delay(_sendDelay);
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

        protected async Task<bool> SendOutputValueVirtualAsync(int virtualChannel, int channel1, int channel2, int value1, int value2)
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

                        await Task.Delay(_sendDelay);
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

        protected async Task<bool> SendServoOutputValueAsync(int channel, int value, int maxValue)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    var servoValue = maxValue * value / 100;
                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);

                    if (_bleDevice?.WriteNoResponse(_characteristic, _servoSendBuffer) == true)
                    {
                        _lastOutputValues[channel] = value;

                        await Task.Delay(_sendDelay);
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

        private async Task<bool> SetupChannelForPortInformationAsync(int channel)
        {
            try
            {
                var lockBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x02 };
                var inputFormatForAbsAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x03, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var inputFormatForRelAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x02, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var modeAndDataSetBuffer = new byte[] { 0x08, 0x00, 0x42, (byte)channel, 0x01, 0x00, 0x30, 0x20 };
                var unlockAndEnableBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x03 };

                return
                    await _bleDevice?.WriteAsync(_characteristic, lockBuffer) &&
                    await _bleDevice?.WriteAsync(_characteristic, inputFormatForAbsAngleBuffer) &&
                    await _bleDevice?.WriteAsync(_characteristic, inputFormatForRelAngleBuffer) &&
                    await _bleDevice?.WriteAsync(_characteristic, modeAndDataSetBuffer) &&
                    await _bleDevice?.WriteAsync(_characteristic, unlockAndEnableBuffer);
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> ResetServoAsync(int channel, int baseAngle)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var absAngle = _absolutePositions[channel];
                var resetToAngle = baseAngle - absAngle;
                
                var ra0 = (byte)(resetToAngle & 0xff);
                var ra1 = (byte)((resetToAngle >> 8) & 0xff);
                var ra2 = (byte)((resetToAngle >> 16) & 0xff);
                var ra3 = (byte)((resetToAngle >> 24) & 0xff);

                var resetToZeroBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, 0x00, 0x00, 0x00, 0x00 };
                var stopBuffer = new byte[] { 0x08, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x00, 0x00 };
                var turnToZeroBuffer = new byte[] { 0x0e, 0x00, 0x81, (byte)channel, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, CalculateServoSpeed(0, 0), 0x64, 0x7e, 0x00 };
                var resetToBaseBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, ra0, ra1, ra2, ra3 };

                var result = true;

                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, resetToZeroBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, resetToBaseBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer);
                await Task.Delay(500);
                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer);

                return result;
            }
            catch
            {
                return false;
            }
        }

        private byte CalculateServoSpeed(int currentAngle, int TargetAngle)
        {
            // Temp
            return 40;
        }
    }
}
