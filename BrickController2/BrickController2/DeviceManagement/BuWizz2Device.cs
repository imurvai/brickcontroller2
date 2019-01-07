using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizz2Device : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("4e050000-74fb-4481-88b3-9919b1676e93");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("000092d1-0000-1000-8000-00805f9b34fb");

        private readonly byte[] _sendOutputBuffer = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private readonly byte[] _sendOutputLevelBuffer = new byte[] { 0x11, 0x00 };
        private readonly int[] _outputValues = new int[4];

        private volatile int _outputLevelValue;
        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public BuWizz2Device(string name, string address, IDeviceRepository deviceRepository, IUIThreadService uiThreadService, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz2;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 4;
        public override int DefaultOutputLevel => 1;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        public override void SetOutputLevel(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
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
            _outputLevelValue = DefaultOutputLevel;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            var _lastSentOutputLevelValue = -1;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_lastSentOutputLevelValue != _outputLevelValue)
                    {
                        if (await SendOutputLevelValueAsync(_outputLevelValue))
                        {
                            _lastSentOutputLevelValue = _outputLevelValue;
                        }
                    }
                    else if (_sendAttemptsLeft > 0)
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
                _sendOutputBuffer[1] = (byte)(v0 / 2);
                _sendOutputBuffer[2] = (byte)(v1 / 2);
                _sendOutputBuffer[3] = (byte)(v2 / 2);
                _sendOutputBuffer[4] = (byte)(v3 / 2);

                await _bleDevice?.WriteAsync(_characteristic, _sendOutputBuffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendOutputLevelValueAsync(int outputLevelValue)
        {
            try
            {
                _sendOutputLevelBuffer[1] = (byte)(outputLevelValue + 1);

                await _bleDevice?.WriteAsync(_characteristic, _sendOutputLevelBuffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
