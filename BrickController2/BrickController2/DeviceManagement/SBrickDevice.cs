using BrickController2.Helpers;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("4dc591b0-857c-41de-b5f1-15abda665b0c");
        private readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");

        private readonly AsyncLock _asyncLock = new AsyncLock();

        private readonly int[] _outputValues = new int[4];

        private ICharacteristic _characteristic;

        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;
        private object _outputTaskLock = new object();
        private int _sendAttemptsLeft;

        public SBrickDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;

        public override async Task SetOutputAsync(int channel, int value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            if (_outputValues[channel] == value)
            {
                return;
            }

            _outputValues[channel] = value;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        protected override async Task<bool> ProcessServices(IList<IService> services)
        {
            _characteristic = null;
            foreach (var service in services)
            {
                if (service.Id == SERVICE_UUID_REMOTE_CONTROL)
                {
                    _characteristic = await service.GetCharacteristicAsync(CHARACTERISTIC_UUID_QUICK_DRIVE);
                }
            }

            return _characteristic != null;
        }

        protected override async Task<bool> ConnectPostActionAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                return await StartOutputTaskAsync();
            }
        }

        protected override async Task DisconnectPreActionAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                await StopOutputTaskAsync();
            }
        }

        private async Task<bool> StartOutputTaskAsync()
        {
            await StopOutputTaskAsync();

            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
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

                            //await Task.Delay(60, _outputTaskTokenSource.Token);
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
                byte[] buffer = new[]
                {
                    (byte)((Math.Abs(v0) & 0xfe) | 0x02 | (v0 < 0 ? 1 : 0)),
                    (byte)((Math.Abs(v1) & 0xfe) | 0x02 | (v1 < 0 ? 1 : 0)),
                    (byte)((Math.Abs(v2) & 0xfe) | 0x02 | (v2 < 0 ? 1 : 0)),
                    (byte)((Math.Abs(v3) & 0xfe) | 0x02 | (v3 < 0 ? 1 : 0)),
                };

                return await _characteristic.WriteAsync(buffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
