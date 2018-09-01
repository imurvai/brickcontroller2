using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class BluetoothDevice : Device
    {
        protected readonly IAdapter _adapter;

        protected IDevice _bleDevice;

        public BluetoothDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository)
        {
            _adapter = adapter;
            _adapter.DeviceConnectionLost += DeviceConnectionLostHandler;
        }

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

                    var guid = new Guid(Address);
                    _bleDevice = await _adapter.ConnectToKnownDeviceAsync(guid, ConnectParameters.None, token);
                    await _bleDevice.GetServicesAsync(token);

                    SetState(DeviceState.Connected, false);
                }
                catch (OperationCanceledException)
                {
                    await DisconnectInternalAsync();
                    return DeviceConnectionResult.Canceled;
                }
                catch (DeviceConnectionException)
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

                await _adapter.DisconnectDeviceAsync(_bleDevice);
                _bleDevice = null;

                SetState(DeviceState.Disconnected, false);
            }
        }

        private void DeviceConnectionLostHandler(object sender, DeviceErrorEventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine("Device connection lost.");
                SetState(DeviceState.Disconnected, true);
            });
        }
    }
}
