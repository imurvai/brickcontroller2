using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class BluetoothDevice : Device
    {
        protected readonly IAdapter _adapter;

        protected IDevice _bleDevice;
        private IDisposable _bleDeviceDisconnectedSubscription;

        public BluetoothDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository)
        {
            _adapter = adapter;
        }

        protected abstract Task<bool> ServicesDiscovered(IList<IGattService> services, CancellationToken token);
        protected abstract Task<bool> ConnectPostActionAsync(CancellationToken token);
        protected abstract Task DisconnectPreActionAsync(CancellationToken token);

        public async override Task<DeviceConnectionResult> ConnectAsync(CancellationToken token)
        {
            if (_bleDevice != null || DeviceState != DeviceState.Disconnected)
            {
                return DeviceConnectionResult.Error;
            }

            try
            {
                var guid = Guid.Parse(Address);
                _bleDevice = await _adapter.GetKnownDevice(guid).FirstAsync();

                _bleDeviceDisconnectedSubscription = _bleDevice
                    .WhenDisconnected()
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(device =>
                    {
                        if (device != _bleDevice)
                        {
                            return;
                        }

                        if (DeviceState != DeviceState.Discovering && DeviceState != DeviceState.Connected)
                        {
                            return;
                        }

                        CleanUp();
                        SetState(DeviceState.Disconnected, true);
                    });

                var connectionFailedTask = _bleDevice.WhenConnectionFailed().ToTask(token);
                var connectionOkTask = _bleDevice.WhenConnected().Take(1).ToTask(token);

                _bleDevice.Connect(new ConnectionConfig { AutoConnect = false });
                SetState(DeviceState.Connecting, false);

                var result = await Task.WhenAny(connectionFailedTask, connectionOkTask);
                if (result == connectionOkTask)
                {
                    if (!connectionOkTask.IsCanceled)
                    {
                        SetState(DeviceState.Discovering, false);
                        var services = new List<IGattService>();
                        await _bleDevice.DiscoverServices().ForEachAsync(service => services.Add(service), token);

                        if (await ServicesDiscovered(services, token) && await ConnectPostActionAsync(token))
                        {
                            SetState(DeviceState.Connected, false);
                            return DeviceConnectionResult.Ok;
                        }
                    }
                    else
                    {
                        CleanUp();
                        SetState(DeviceState.Disconnected, false);
                        return DeviceConnectionResult.Canceled;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                CleanUp();
                SetState(DeviceState.Disconnected, false);
                return DeviceConnectionResult.Canceled;
            }
            catch (Exception)
            {
            }

            CleanUp();
            SetState(DeviceState.Disconnected, true);
            return DeviceConnectionResult.Error;
        }

        public async override Task DisconnectAsync()
        {
            if (DeviceState == DeviceState.Disconnected)
            {
                return;
            }

            await DisconnectInternalAsync();
        }

        private async Task DisconnectInternalAsync()
        {
            if (_bleDevice != null)
            {
                SetState(DeviceState.Disconnecting, false);
                await DisconnectPreActionAsync(CancellationToken.None);
            }

            CleanUp();
            SetState(DeviceState.Disconnected, false);
        }

        private void CleanUp()
        {
            _bleDeviceDisconnectedSubscription?.Dispose();
            _bleDeviceDisconnectedSubscription = null;
            _bleDevice?.CancelConnection();
            _bleDevice = null;
        }
    }
}
