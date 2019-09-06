using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class ControlPlusDevice : BluetoothDevice
    {
        protected const int MAX_SEND_ATTEMPTS = 4;

        protected readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        protected readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        protected readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        protected readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        protected readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };
        protected readonly byte[] _virtualPortSetupBuffer = new byte[] { 6, 0x00, 0x61, 0x01, 0x00, 0x00 };

        protected readonly int[] _outputValues = new int[4];
        protected readonly int[] _maxServoAngles = new int[4];
        protected readonly int[] _lastOutputValues = new int[4];

        protected volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;
        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        protected override bool AutoConnectOnFirstConnect => true;

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

        //public override void SetOutputMaxServoAngle(int channel, int maxServoAngle)
        //{
        //    CheckChannel(channel);
        //    _maxServoAngles[channel] = Math.Min(maxServoAngle, 360);
        //}

        protected override bool ValidateServices(IEnumerable<IGattService> services)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return _characteristic != null;
        }

        protected async Task<bool> SetupVirtualPortAsync(int channel1, int channel2)
        {
            try
            {
                _virtualPortSetupBuffer[4] = (byte)channel1;
                _virtualPortSetupBuffer[5] = (byte)channel2;

                return await _bleDevice?.WriteAsync(_characteristic, _virtualPortSetupBuffer);
            }
            catch
            {
                return false;
            }
        }

        protected async Task<bool> SendOutputValueAsync(int channel, int value)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    _sendBuffer[3] = (byte)channel;
                    _sendBuffer[7] = (byte)(value < 0 ? (255 + value) : value);

                    if (await _bleDevice?.WriteAsync(_characteristic, _sendBuffer))
                    {
                        _lastOutputValues[channel] = value;
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

                    if (await _bleDevice?.WriteAsync(_characteristic, _virtualPortSendBuffer))
                    {
                        _lastOutputValues[channel1] = value1;
                        _lastOutputValues[channel2] = value2;
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

                    if (await WriteRawServoValueAsync(channel, servoValue))
                    {
                        _lastOutputValues[channel] = value;
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

        private async Task<bool> WriteRawServoValueAsync(int channel, int rawValue)
        {
            try
            {
                _servoSendBuffer[3] = (byte)channel;
                _servoSendBuffer[6] = (byte)(rawValue & 0xff);
                _servoSendBuffer[7] = (byte)((rawValue >> 8) & 0xff);
                _servoSendBuffer[8] = (byte)((rawValue >> 16) & 0xff);
                _servoSendBuffer[9] = (byte)((rawValue >> 24) & 0xff);

                return await _bleDevice?.WriteAsync(_characteristic, _servoSendBuffer);
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
        
        private async Task<bool> ResetServoAsync(int channel, int baseAngle, int currentAbsAngle)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var ba0 = (byte)(currentAbsAngle - baseAngle & 0xff);
                var ba1 = (byte)((currentAbsAngle - baseAngle >> 8) & 0xff);
                var ba2 = (byte)((currentAbsAngle - baseAngle >> 16) & 0xff);
                var ba3 = (byte)((currentAbsAngle - baseAngle >> 24) & 0xff);

                var resetToZeroBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, 0x00, 0x00, 0x00, 0x00 };
                var stopBuffer = new byte[] { 0x08, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x00, 0x00 };
                var turnToZeroBuffer = new byte[] { 0x0e, 0x00, 0x81, (byte)channel, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, CalculateServoSpeed(0, 0), 0x64, 0x7e, 0x00 };
                var resetToBaseBuffer = new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, ba0, ba1, ba2, ba3 };

                var result = true;

                result = result && await _bleDevice.WriteAsync(_characteristic, resetToBaseBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, stopBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, resetToBaseBuffer);
                result = result && await _bleDevice.WriteAsync(_characteristic, turnToZeroBuffer);
                await Task.Delay(500);

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
            return 100;
        }
    }
}
