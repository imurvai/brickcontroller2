using BrickController2.Helpers;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _adapter.DeviceConnectionLost += DeviceConnectionLostHandler;
        }

        protected abstract Task<bool> ProcessServices(IList<IService> services);
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
                    _bleDevice = await _adapter.ConnectToKnownDeviceAsync(guid, ConnectParameters.None, token);
                    var services = await _bleDevice.GetServicesAsync(token);

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
                if (DeviceState == DeviceState.Disconnected || DeviceState == DeviceState.Disconnecting)
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

                if (_bleDevice.State != Plugin.BLE.Abstractions.DeviceState.Disconnected)
                {
                    await _adapter.DisconnectDeviceAsync(_bleDevice);
                }

                _bleDevice.Dispose();
                _bleDevice = null;
            }

            SetState(DeviceState.Disconnected, false);
        }

        private void DeviceConnectionLostHandler(object sender, DeviceErrorEventArgs e)
        {
            if (e.Device != _bleDevice)
            {
                return;
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine("Device connection lost.");
                SetState(DeviceState.Disconnected, true);
            });
        }
    }
}
