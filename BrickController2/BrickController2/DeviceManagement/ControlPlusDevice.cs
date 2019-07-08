using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
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

        private readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        private readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 100, 100, 127, 0x00 };
        private readonly int[] _outputValues = new int[4];
        private readonly int[] _lastOutputValues = new int[4];

        private volatile int _sendAttemptsLeft;

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

        protected override bool ProcessServices(IEnumerable<IGattService> services)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return _characteristic != null;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
            _lastOutputValues[0] = 0;
            _lastOutputValues[1] = 0;
            _lastOutputValues[2] = 0;
            _lastOutputValues[3] = 0;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        int v0 = _outputValues[0];
                        int v1 = _outputValues[1];
                        int v2 = _outputValues[2];
                        int v3 = _outputValues[3];

                        if (await SendOutputValuesAsync(v0, v1, v2, v3))
                        {
                            if (v0 != 0 || v1 != 0 || v2 != 0 || v3 != 0)
                            {
                                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                            }
                            else
                            {
                                _sendAttemptsLeft--;
                            }
                        }
                        else
                        {
                            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                        }
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3)
        {
            try
            {
                await SendOutputValueAsync(v0, 0, -1);
                await SendOutputValueAsync(v1, 1, -1);
                await SendOutputValueAsync(v2, 2, -1);
                await SendOutputValueAsync(v3, 3, -1);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueAsync(int value, int channel, int maxServoAngle)
        {
            if (_lastOutputValues[channel] != value)
            {
                //if (maxServoAngle < 0)
                if (channel != 3)
                {
                    _sendBuffer[3] = (byte)channel;
                    _sendBuffer[7] = (byte)(value < 0 ? (255 + value) : value);

                    if (await _bleDevice?.WriteAsync(_characteristic, _sendBuffer))
                    {
                        _lastOutputValues[channel] = value;
                        return true;
                    }
                }
                else
                {
                    var servoValue = 30 * value / 100;
                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 32) & 0xff);

                    if (await _bleDevice?.WriteAsync(_characteristic, _servoSendBuffer))
                    {
                        _lastOutputValues[channel] = value;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
