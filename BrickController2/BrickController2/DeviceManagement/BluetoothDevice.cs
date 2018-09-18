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

        protected abstract Task<bool> ServicesDiscovered(IList<IGattService> services);
        protected abstract Task<bool> ConnectPostActionAsync();
        protected abstract Task DisconnectPreActionAsync();

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
                    SetState(DeviceState.Connecting, false);

                    var guid = Guid.Parse(Address);
                    _bleDevice = await _adapter.GetKnownDevice(guid).FirstAsync();

                    var connectionFailedTask = _bleDevice.WhenConnectionFailed().ToTask(token);
                    var connectionOkTask = _bleDevice.WhenConnected().Take(1).ToTask(token);
                    _bleDevice.Connect(new ConnectionConfig { AutoConnect = false });

                    var result = await Task.WhenAny(connectionFailedTask, connectionOkTask);
                    if (result == connectionOkTask)
                    {
                        if (!connectionOkTask.IsCanceled)
                        {
                            _bleDeviceDisconnectedSubscription = _bleDevice
                                .WhenDisconnected()
                                .Take(1)
                                .ObserveOn(SynchronizationContext.Current)
                                .Subscribe(device =>
                                {
                                    if (device == _bleDevice)
                                    {
                                        CleanUp();
                                        SetState(DeviceState.Disconnected, true);
                                    }
                                });

                            var services = new List<IGattService>();
                            await _bleDevice.DiscoverServices().ForEachAsync(service => services.Add(service), token);

                            if (await ServicesDiscovered(services) && await ConnectPostActionAsync())
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
        }

        public async override Task DisconnectAsync()
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

        private async Task DisconnectInternalAsync()
        {
            if (_bleDevice != null)
            {
                SetState(DeviceState.Disconnecting, false);
                await DisconnectPreActionAsync();
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
