using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : BluetoothGattCallback, IBluetoothLEDevice
    {
        private readonly Context _context;
        private readonly BluetoothDevice _bluetoothDevice;
        private readonly AsyncLock _lock = new AsyncLock();

        private BluetoothGatt _bluetoothGatt = null;

        private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
        private TaskCompletionSource<bool> _writeCompletionSource = null;

        public BluetoothLEDevice(Context context, BluetoothDevice bluetoothDevice)
        {
            _context = context;
            _bluetoothDevice = bluetoothDevice;
        }

        public string Address => _bluetoothDevice.Address;
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

        public event EventHandler<EventArgs> Disconnected;

        public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(CancellationToken token)
        {
            using (await _lock.LockAsync())
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return null;
                }

                State = BluetoothLEDeviceState.Connecting;

                _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, true, this);
                if (_bluetoothGatt == null)
                {
                    State = BluetoothLEDeviceState.Disconnected;
                    return null;
                }

                _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>();
                token.Register(async () =>
                {
                    using (await _lock.LockAsync())
                    {
                        DisconnectInternal();
                        _connectCompletionSource?.SetResult(null);
                    }
                });

                var result = await _connectCompletionSource.Task;
                _connectCompletionSource = null;
                return result;
            }
        }

        public async Task DisconnectAsync()
        {
            using (await _lock.LockAsync())
            {
                DisconnectInternal();
            }
        }

        private void DisconnectInternal()
        {
            if (_bluetoothGatt != null)
            {
                _bluetoothGatt.Disconnect();
                _bluetoothGatt.Dispose();
                _bluetoothGatt = null;
            }

            State = BluetoothLEDeviceState.Disconnected;
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data)
        {
            using (await _lock.LockAsync())
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var gattCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                gattCharacteristic.WriteType = GattWriteType.Default;

                if (!gattCharacteristic.SetValue(data))
                {
                    return false;
                }

                _writeCompletionSource = new TaskCompletionSource<bool>();

                if (!_bluetoothGatt.WriteCharacteristic(gattCharacteristic))
                {
                    _writeCompletionSource = null;
                    return false;
                }

                var result = await _writeCompletionSource.Task;
                _writeCompletionSource = null;
                return result;
            }
        }

        public async Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data)
        {
            using (await _lock.LockAsync())
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var gattCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                gattCharacteristic.WriteType = GattWriteType.NoResponse;

                if (!gattCharacteristic.SetValue(data))
                {
                    return false;
                }

                if (!_bluetoothGatt.WriteCharacteristic(gattCharacteristic))
                {
                    return false;
                }

                return true;
            }
        }

        public override async void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            if (status != GattStatus.Success)
            {
                return;
            }

            switch (newState)
            {
                case ProfileState.Connecting:
                    break;

                case ProfileState.Connected:
                    using (await _lock.LockAsync())
                    {
                        if (State == BluetoothLEDeviceState.Connecting)
                        {
                            State = BluetoothLEDeviceState.Discovering;
                        }
                        else
                        {
                            return;
                        }
                    }

                    await Task.Delay(750);

                    using (await _lock.LockAsync())
                    {
                        if (State == BluetoothLEDeviceState.Discovering && _bluetoothGatt != null)
                        {
                            if (!_bluetoothGatt.DiscoverServices())
                            {
                                DisconnectInternal();
                                _connectCompletionSource?.SetResult(null);
                                _connectCompletionSource = null;
                            }
                        }
                    }

                    break;

                case ProfileState.Disconnecting:
                    break;

                case ProfileState.Disconnected:
                    using (await _lock.LockAsync())
                    {
                        switch (State)
                        {
                            case BluetoothLEDeviceState.Connecting:
                            case BluetoothLEDeviceState.Discovering:
                                DisconnectInternal();
                                _connectCompletionSource?.SetResult(null);
                                break;

                            case BluetoothLEDeviceState.Connected:
                                _writeCompletionSource?.SetResult(false);
                                DisconnectInternal();
                                Disconnected?.Invoke(this, EventArgs.Empty);
                                break;

                            default:
                                break;
                        }
                    }

                    break;
            }
        }

        public override async void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            using (await _lock.LockAsync())
            {
                if (status == GattStatus.Success && State == BluetoothLEDeviceState.Disconnecting)
                {
                    var services = new List<GattService>();
                    if (gatt.Services != null)
                    {
                        foreach (var service in gatt.Services)
                        {
                            var characteristics = new List<GattCharacteristic>();
                            if (service.Characteristics != null)
                            {
                                foreach (var characteristic in service.Characteristics)
                                {
                                    characteristics.Add(new GattCharacteristic(characteristic));
                                }
                            }

                            services.Add(new GattService(service, characteristics));
                        }
                    }

                    State = BluetoothLEDeviceState.Connected;
                    _connectCompletionSource?.SetResult(services);
                }
                else
                {
                    DisconnectInternal();
                    _connectCompletionSource?.SetResult(null);
                }
            }
        }

        public override async void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            using (await _lock.LockAsync())
            {
                _writeCompletionSource?.SetResult(status == GattStatus.Success);
            }
        }
    }
}