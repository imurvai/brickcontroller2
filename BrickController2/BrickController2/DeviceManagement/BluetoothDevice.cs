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
    internal abstract class BluetoothDevice : Device
    {
        protected readonly IAdapter _adapter;
        protected readonly AsyncLock _asyncLock = new AsyncLock();

        protected IDevice _bleDevice;

        public BluetoothDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository)
        {
            _adapter = adapter;
        }

        protected abstract Task<bool> ProcessServices(IList<IGattService> services);
        protected abstract Task<bool> ConnectPostActionAsync();
        protected abstract Task DisconnectPreActionAsync();

        public async override Task<DeviceConnectionResult> ConnectAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (DeviceState != DeviceState.Disconnected)
                {
                    return DeviceConnectionResult.Error;
                }

                try
                {
                    SetState(DeviceState.Connecting, false);

                    var guid = Guid.Parse(Address);
                    _bleDevice = await _adapter.GetKnownDevice(guid).FirstAsync().ToTask();
                    // TODO: Setup the state changed event!!!

                    await _bleDevice.ConnectWait().ToTask(token);

                    var services = await _bleDevice.DiscoverServices().ToList().ToTask();

                    if (await ProcessServices(services))
                    {
                        await ConnectPostActionAsync();
                        SetState(DeviceState.Connected, false);
                    }
                    else
                    {
                        await DisconnectInternalAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    await DisconnectInternalAsync();
                    return DeviceConnectionResult.Canceled;
                }
                catch (Exception)
                {
                    SetState(DeviceState.Disconnected, true);
                    return DeviceConnectionResult.Error;
                }

                return DeviceConnectionResult.Ok;
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

                _bleDevice.CancelConnection();
                _bleDevice = null;
            }

            SetState(DeviceState.Disconnected, false);
        }

    }
}
