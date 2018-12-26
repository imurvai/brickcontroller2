using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly byte[] _sendBufferAB = new byte[] { 0x08, 0x00, 0x81, 0x39, 0x11, 0x02, 0x00, 0x00 };
        private readonly byte[] _sendBufferCD = new byte[] { 0x0a, 0x00, 0x81, 0x00, 0x11, 0x01, 0x00, 0x64, 0x7f, 0x03 };
        private readonly int[] _outputValues = new int[4];
        private readonly int[] _lastOutputValues = new int[4];

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public BoostDevice(string name, string address, IDeviceRepository deviceRepository, IUIThreadService uiThreadService, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;

        public override int NumberOfChannels => 4;

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
                if (_lastOutputValues[0] != v0 || _lastOutputValues[1] != v1)
                {
                    _sendBufferAB[6] = (byte)(v0 < 0 ? (255 + v0) : v0);
                    _sendBufferAB[7] = (byte)(v1 < 0 ? (255 + v1) : v1);

                    await _bleDevice?.WriteAsync(_characteristic, _sendBufferAB);

                    _lastOutputValues[0] = v0;
                    _lastOutputValues[1] = v1;
                }

                if (_lastOutputValues[2] != v2)
                {
                    _sendBufferCD[3] = 1;
                    _sendBufferCD[6] = (byte)(v2 < 0 ? (255 + v2) : v2);

                    await _bleDevice?.WriteAsync(_characteristic, _sendBufferCD);

                    _lastOutputValues[2] = v2;
                }

                if (_lastOutputValues[3] != v3)
                {
                    _sendBufferCD[3] = 2;
                    _sendBufferCD[6] = (byte)(v3 < 0 ? (255 + v3) : v3);

                    await _bleDevice?.WriteAsync(_characteristic, _sendBufferCD);

                    _lastOutputValues[3] = v3;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
