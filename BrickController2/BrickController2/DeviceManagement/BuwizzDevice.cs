using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizzDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private static readonly Guid SERVICE_UUID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");

        private readonly int[] _outputValues = new int[4];
        private readonly int[] _lastOutputValues = new int[4];
        private readonly object _outputLock = new object();

        private volatile int _outputLevelValue;
        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public BuWizzDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 3;
        public override int DefaultOutputLevel => 1;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);

            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        public override bool CanSetOutputLevel => true;

        public override void SetOutputLevel(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
        }

        public override bool CanBePowerSource => true;

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return Task.FromResult(_characteristic != null);
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            lock (_outputLock)
            {
                _outputValues[0] = 0;
                _outputValues[1] = 0;
                _outputValues[2] = 0;
                _outputValues[3] = 0;
                _lastOutputValues[0] = 1;
                _lastOutputValues[1] = 1;
                _lastOutputValues[2] = 1;
                _lastOutputValues[3] = 1;
                _outputLevelValue = DefaultOutputLevel;
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int v0, v1, v2, v3, sendAttemptsLeft;

                    lock (_outputLock)
                    {
                        v0 = _outputValues[0];
                        v1 = _outputValues[1];
                        v2 = _outputValues[2];
                        v3 = _outputValues[3];
                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }

                    if (v0 != _lastOutputValues[0] || v1 != _lastOutputValues[1] || v2 != _lastOutputValues[2] || v3 != _lastOutputValues[3] || sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(v0, v1, v2, v3, token).ConfigureAwait(false))
                        {
                            _lastOutputValues[0] = v0;
                            _lastOutputValues[1] = v1;
                            _lastOutputValues[2] = v2;
                            _lastOutputValues[3] = v3;
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
                catch
                {
                }
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                var sendOutputBuffer = new byte[]
                {
                    (byte)((Math.Abs(v0) >> 2) | (v0 < 0 ? 0x40 : 0) | 0x80),
                    (byte)((Math.Abs(v1) >> 2) | (v1 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v2) >> 2) | (v2 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v3) >> 2) | (v3 < 0 ? 0x40 : 0)),
                    (byte)(_outputLevelValue * 0x20)
                };

                var result = await _bleDevice?.WriteNoResponseAsync(_characteristic, sendOutputBuffer, token);
                await Task.Delay(60, token);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}