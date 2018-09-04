using BrickController2.Helpers;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizzDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");

        private readonly AsyncLock _asyncLock = new AsyncLock();

        private readonly int[] _outputValues = new int[4];
        private int _outputLevelValue;

        private IGattCharacteristic _characteristic;

        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;
        private object _outputTaskLock = new object();
        private int _sendAttemptsLeft;

        public BuWizzDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 3;
        public override int DefaultOutputLevel => 2;

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

        public override async Task SetOutputLevelAsync(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
        }

        protected override async Task<bool> ProcessServices(IList<IGattService> services)
        {
            _characteristic = null;
            foreach (var service in services)
            {
                if (service.Uuid == SERVICE_UUID_REMOTE_CONTROL)
                {
                    _characteristic = await service.GetKnownCharacteristics(CHARACTERISTIC_UUID_QUICK_DRIVE).FirstAsync().ToTask();
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
            _outputLevelValue = DefaultOutputLevel;
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

                            await Task.Delay(60, _outputTaskTokenSource.Token);
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
                    (byte)((Math.Abs(v0) >> 2) | (v0 < 0 ? 0x40 : 0) | 0x80),
                    (byte)((Math.Abs(v1) >> 2) | (v1 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v2) >> 2) | (v2 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v3) >> 2) | (v3 < 0 ? 0x40 : 0)),
                    (byte)_outputLevelValue
                };

                await _characteristic.WriteWithoutResponse(buffer).ToTask(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}