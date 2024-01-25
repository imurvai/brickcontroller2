using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal abstract class BluetoothDevice : Device
    {
        protected readonly IBluetoothLEService _bleService;

        protected IBluetoothLEDevice _bleDevice;
        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;

        private Action<Device> _onDeviceDisconnected = null;

        public BluetoothDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository)
        {
            _bleService = bleService;
        }

        protected abstract bool AutoConnectOnFirstConnect { get; }
        protected abstract Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token);
        protected abstract Task ProcessOutputsAsync(CancellationToken token);

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (_bleDevice != null || DeviceState != DeviceState.Disconnected)
                {
                    return DeviceConnectionResult.Error;
                }

                _onDeviceDisconnected = onDeviceDisconnected;

                try
                {
                    _bleDevice = _bleService.GetKnownDevice(Address);
                    if (_bleDevice == null)
                    {
                        return DeviceConnectionResult.Error;
                    }

                    DeviceState = DeviceState.Connecting;
                    var services = await _bleDevice.ConnectAndDiscoverServicesAsync(
                        reconnect || AutoConnectOnFirstConnect,
                        OnCharacteristicChanged,
                        OnDeviceDisconnected,
                        token);

                    token.ThrowIfCancellationRequested();

                    if (await ValidateServicesAsync(services, token) &&
                        await AfterConnectSetupAsync(requestDeviceInformation, token))
                    {
                        if (startOutputProcessing)
                        {
                            await StartOutputTaskAsync();
                        }

                        token.ThrowIfCancellationRequested();

                        DeviceState = DeviceState.Connected;
                        return DeviceConnectionResult.Ok;
                    }
                }
                catch (OperationCanceledException)
                {
                    await DisconnectInternalAsync();
                    return DeviceConnectionResult.Canceled;
                }
                catch (Exception)
                {
                }

                await DisconnectInternalAsync();
                return DeviceConnectionResult.Error;
            }
        }

        public override async Task DisconnectAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (DeviceState == DeviceState.Disconnected)
                {
                    return;
                }

                await DisconnectInternalAsync();
            }
        }

        protected virtual void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
        }

        protected virtual Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        private async Task DisconnectInternalAsync()
        {
            if (_bleDevice != null)
            {
                await StopOutputTaskAsync();
                DeviceState = DeviceState.Disconnecting;
                await _bleDevice.DisconnectAsync();
                _bleDevice = null;
            }

            DeviceState = DeviceState.Disconnected;
        }

        private void OnDeviceDisconnected(IBluetoothLEDevice bluetoothLEDevice)
        {
            Task.Run(async () =>
            {
                using (await _asyncLock.LockAsync())
                {
                    await DisconnectInternalAsync();
                    _onDeviceDisconnected?.Invoke(this);
                }
            });
        }

        private async Task StartOutputTaskAsync()
        {
            await StopOutputTaskAsync();

            _outputTaskTokenSource = new CancellationTokenSource();
            var token = _outputTaskTokenSource.Token;

            _outputTask = Task.Run(async () =>
            {
                try
                {
                    await ProcessOutputsAsync(token).ConfigureAwait(false);
                }
                catch
                {
                }
            });
        }

        private async Task StopOutputTaskAsync()
        {
            if (_outputTaskTokenSource != null && _outputTask != null)
            {
                _outputTaskTokenSource.Cancel();
                await _outputTask;
                _outputTaskTokenSource.Dispose();
                _outputTaskTokenSource = null;
                _outputTask = null;
            }
        }
    }
}
