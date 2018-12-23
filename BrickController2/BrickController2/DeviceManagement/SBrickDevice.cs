using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("4dc591b0-857c-41de-b5f1-15abda665b0c");
        private readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");

        private readonly byte[] _sendBuffer = new byte[4];
        private readonly int[] _outputValues = new int[4];

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public SBrickDevice(string name, string address, IDeviceRepository deviceRepository, IUIThreadService uiThreadService, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;

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

        protected override bool ProcessServices(IEnumerable<IGattService> services)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_REMOTE_CONTROL);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_QUICK_DRIVE);

            return _characteristic != null;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
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
                _sendBuffer[0] = (byte)((Math.Abs(v0) & 0xfe) | 0x02 | (v0 < 0 ? 1 : 0));
                _sendBuffer[1] = (byte)((Math.Abs(v1) & 0xfe) | 0x02 | (v1 < 0 ? 1 : 0));
                _sendBuffer[2] = (byte)((Math.Abs(v2) & 0xfe) | 0x02 | (v2 < 0 ? 1 : 0));
                _sendBuffer[3] = (byte)((Math.Abs(v3) & 0xfe) | 0x02 | (v3 < 0 ? 1 : 0));

                await _bleDevice?.WriteAsync(_characteristic, _sendBuffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
