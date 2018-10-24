using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
        private int _outputLevelValue;

        private IGattCharacteristic _characteristic;

        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;
        private object _outputTaskLock = new object();
        private int _sendAttemptsLeft;

        public BuWizz2Device(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz2;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 4;
        public override int DefaultOutputLevel => 1;

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

        protected override async Task<bool> ServicesDiscovered(IList<IGattService> services, CancellationToken token)
        {
            _characteristic = null;
            foreach (var service in services)
            {
                if (service.Uuid == SERVICE_UUID)
                {
                    _characteristic = await service.GetKnownCharacteristics(CHARACTERISTIC_UUID).FirstAsync().ToTask(token);
                }
            }

            return _characteristic != null;
        }

        protected override async Task<bool> ConnectPostActionAsync(CancellationToken token)
        {
            return await StartOutputTaskAsync();
        }

        protected override async Task DisconnectPreActionAsync(CancellationToken token)
        {
            await StopOutputTaskAsync();
        }

        private async Task<bool> StartOutputTaskAsync()
        {
            await StopOutputTaskAsync();

            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
            _outputLevelValue = DefaultOutputLevel;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            _outputTaskTokenSource = new CancellationTokenSource();
            _outputTask = Task.Run(async () =>
            {
                var _lastSentOutputLevelValue = -1;

                while (!_outputTaskTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_lastSentOutputLevelValue != _outputLevelValue)
                        {
                            if (await SendOutputLevelValueAsync(_outputLevelValue, _outputTaskTokenSource.Token))
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

                            if (await SendOutputValuesAsync(v0, v1, v2, v3, _outputTaskTokenSource.Token))
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
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            });

            return true;
        }

        private async Task StopOutputTaskAsync()
        {
            if (_outputTaskTokenSource != null)
            {
                _outputTaskTokenSource.Cancel();
                await _outputTask;
                _outputTaskTokenSource.Dispose();
                _outputTaskTokenSource = null;
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                _sendOutputBuffer[1] = (byte)(v0 / 2);
                _sendOutputBuffer[2] = (byte)(v1 / 2);
                _sendOutputBuffer[3] = (byte)(v2 / 2);
                _sendOutputBuffer[4] = (byte)(v3 / 2);

                await _characteristic.Write(_sendOutputBuffer).ToTask(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendOutputLevelValueAsync(int outputLevelValue, CancellationToken token)
        {
            try
            {
                _sendOutputLevelBuffer[1] = (byte)(outputLevelValue + 1);

                await _characteristic.Write(_sendOutputLevelBuffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
