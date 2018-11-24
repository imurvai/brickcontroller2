using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class BluetoothDevice : Device
    {
        protected readonly IBluetoothLEService _bleService;

        protected IBluetoothLEDevice _bleDevice;
        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;

        public BluetoothDevice(string name, string address, IDeviceRepository deviceRepository, IUIThreadService uiThreadService, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService)
        {
            _bleService = bleService;
        }

        protected abstract bool ProcessServices(IEnumerable<IGattService> services);
        protected abstract Task ProcessOutputsAsync(CancellationToken token);

        public async override Task<DeviceConnectionResult> ConnectAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (_bleDevice != null || DeviceState != DeviceState.Disconnected)
                {
                    return DeviceConnectionResult.Error;
                }

                try
                {
                    _bleDevice = _bleService.GetKnownDevice(Address);
                    if (_bleDevice == null)
                    {
                        return DeviceConnectionResult.Error;
                    }

                    _bleDevice.Disconnected += OnDeviceDisconnected;

                    await SetStateAsync(DeviceState.Connecting, false);
                    var services = await _bleDevice.ConnectAndDiscoverServicesAsync(token);

                    token.ThrowIfCancellationRequested();

                    if (ProcessServices(services))
                    {
                        await StartOutputTaskAsync();

                        token.ThrowIfCancellationRequested();


                        await SetStateAsync(DeviceState.Connected, false);
                        return DeviceConnectionResult.Ok;
                    }
                }
                catch (OperationCanceledException)
                {
                    await DisconnectInternalAsync(false);
                    return DeviceConnectionResult.Canceled;
                }
                catch (Exception)
                {
                }

                await DisconnectInternalAsync(true);
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

                await DisconnectInternalAsync(false);
            }
        }

        private async Task DisconnectInternalAsync(bool isError)
        {
            if (_bleDevice != null)
            {
                await StopOutputTaskAsync();
                await SetStateAsync(DeviceState.Disconnecting, isError);
                _bleDevice.Disconnected -= OnDeviceDisconnected;
                _bleDevice?.Disconnect();
                _bleDevice = null;
            }

            await SetStateAsync(DeviceState.Disconnected, isError);
        }

        private async void OnDeviceDisconnected(object sender, EventArgs args)
        {
            using (await _asyncLock.LockAsync())
            {
                await DisconnectInternalAsync(true);
            }
        }

        private async Task StartOutputTaskAsync()
        {
            await StopOutputTaskAsync();

            _outputTaskTokenSource = new CancellationTokenSource();
            _outputTask = Task.Run(async () =>
            {
                try
                {
                    await ProcessOutputsAsync(_outputTaskTokenSource.Token);
                }
                catch (Exception)
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
