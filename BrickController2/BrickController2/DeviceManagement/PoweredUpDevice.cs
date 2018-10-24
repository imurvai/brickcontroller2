using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly byte[] _sendBuffer = new byte[] { 0x0a, 0x00, 0x81, 0x00, 0x11, 0x60, 0x00, 0x00, 0x00, 0x00 };
        private readonly int[] _outputValues = new int[2];

        private IGattCharacteristic _characteristic;

        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;
        private object _outputTaskLock = new object();
        private int _sendAttemptsLeft;

        public PoweredUpDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;

        public override int NumberOfChannels => 2;

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
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            _outputTaskTokenSource = new CancellationTokenSource();
            _outputTask = Task.Run(async () =>
            {
                while (!_outputTaskTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_sendAttemptsLeft > 0)
                        {
                            int v0 = _outputValues[0];
                            int v1 = _outputValues[1];

                            if (await SendOutputValuesAsync(v0, v1, _outputTaskTokenSource.Token))
                            {
                                if (v0 != 0 || v1 != 0)
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

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, CancellationToken token)
        {
            try
            {
                _sendBuffer[3] = (byte)0x00;
                _sendBuffer[7] = (byte)(v0 < 0 ? (255 + v0) : v0);

                await _characteristic.Write(_sendBuffer);

                _sendBuffer[3] = (byte)0x01;
                _sendBuffer[7] = (byte)(v1 < 0 ? (255 + v1) : v1);

                await _characteristic.Write(_sendBuffer);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
