using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
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
        protected readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 127, 0x00 };
        protected readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };
        protected readonly byte[] _virtualPortSetupBuffer = new byte[] { 6, 0x00, 0x61, 0x01, 0x00, 0x00 };
        protected readonly int[] _outputValues = new int[4];
        protected readonly int[] _maxServoAngles = new int[4];
        protected readonly int[] _lastOutputValues = new int[4];

        protected volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;
        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IUIThreadService uiThreadService, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
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

        public override void SetOutputMaxServoAngle(int channel, int maxServoAngle)
        {
            CheckChannel(channel);
            _maxServoAngles[channel] = Math.Min(maxServoAngle, 360);
        }

        protected override bool ProcessServices(IEnumerable<IGattService> services)
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
                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);

                    if (await _bleDevice?.WriteAsync(_characteristic, _servoSendBuffer))
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
    }
}
