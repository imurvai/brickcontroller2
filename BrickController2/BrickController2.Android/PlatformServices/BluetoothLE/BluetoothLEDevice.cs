using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : BluetoothGattCallback, IBluetoothLEDevice
    {
        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly object _lock = new object();

        private BluetoothDevice _bluetoothDevice = null;
        private BluetoothGatt _bluetoothGatt = null;

        private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
        private TaskCompletionSource<bool> _writeCompletionSource = null;

        public BluetoothLEDevice(Context context, BluetoothAdapter bluetoothAdapter, string address)
        {
            _context = context;
            _bluetoothAdapter = bluetoothAdapter;
            Address = address;
        }

        public string Address { get; }
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

        public event EventHandler<EventArgs> Disconnected;

        public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(CancellationToken token)
        {
            lock(_lock)
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return null;
                }

                State = BluetoothLEDeviceState.Connecting;

                _bluetoothDevice = _bluetoothAdapter.GetRemoteDevice(Address);
                if (_bluetoothDevice == null)
                {
                    State = BluetoothLEDeviceState.Disconnected;
                    return null;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, true, this, BluetoothTransports.Le);
                }
                else
                {
                    _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, true, this);
                }

                if (_bluetoothGatt == null)
                {
                    _bluetoothDevice.Dispose();
                    _bluetoothDevice = null;
                    State = BluetoothLEDeviceState.Disconnected;
                    return null;
                }

                _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
                token.Register(() =>
                {
                    lock(_lock)
                    {
                        Disconnect();
                        _connectCompletionSource?.SetResult(null);
                    }
                });
            }

            var result = await _connectCompletionSource.Task;
            _connectCompletionSource = null;
            return result;
        }

        public void Disconnect()
        {
            lock(_lock)
            {
                if (_bluetoothGatt != null)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothDevice.Dispose();
                    _bluetoothGatt = null;
                    _bluetoothDevice = null;
                }

                State = BluetoothLEDeviceState.Disconnected;
            }
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data)
        {
            lock (_lock)
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

                _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                if (!_bluetoothGatt.WriteCharacteristic(gattCharacteristic))
                {
                    _writeCompletionSource = null;
                    return false;
                }
            }

            var result = await _writeCompletionSource.Task;
            _writeCompletionSource = null;
            return result;
        }

        public bool WriteNoResponse(IGattCharacteristic characteristic, byte[] data)
        {
            lock (_lock)
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

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            switch (newState)
            {
                case ProfileState.Connecting:
                    break;

                case ProfileState.Connected:
                    lock (_lock)
                    {
                        if (State == BluetoothLEDeviceState.Connecting)
                        {
                            State = BluetoothLEDeviceState.Discovering;
                            Task.Run(async () =>
                            {
                                await Task.Delay(750);
                                lock (_lock)
                                {
                                    if (State == BluetoothLEDeviceState.Discovering && _bluetoothGatt != null)
                                    {
                                        if (!_bluetoothGatt.DiscoverServices())
                                        {
                                            Disconnect();
                                            _connectCompletionSource?.SetResult(null);
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            return;
                        }
                    }

                    break;

                case ProfileState.Disconnecting:
                    break;

                case ProfileState.Disconnected:
                    lock (_lock)
                    {
                        switch (State)
                        {
                            case BluetoothLEDeviceState.Connecting:
                            case BluetoothLEDeviceState.Discovering:
                                Disconnect();
                                _connectCompletionSource?.SetResult(null);
                                break;

                            case BluetoothLEDeviceState.Connected:
                                _writeCompletionSource?.SetResult(false);
                                Disconnect();
                                Disconnected?.Invoke(this, EventArgs.Empty);
                                break;

                            default:
                                break;
                        }
                    }

                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            lock (_lock)
            {
                if (status == GattStatus.Success && State == BluetoothLEDeviceState.Discovering)
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
                    Disconnect();
                    _connectCompletionSource?.SetResult(null);
                }
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            lock (_lock)
            {
                _writeCompletionSource?.SetResult(status == GattStatus.Success);
            }
        }
    }
}