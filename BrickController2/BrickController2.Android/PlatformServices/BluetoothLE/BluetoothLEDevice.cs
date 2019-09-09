﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using BrickController2.PlatformServices.BluetoothLE;
using Java.Util;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : BluetoothGattCallback, IBluetoothLEDevice
    {
        private static readonly UUID ClientCharacteristicConfigurationUUID = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly object _lock = new object();

        private BluetoothDevice _bluetoothDevice = null;
        private BluetoothGatt _bluetoothGatt = null;

        private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
        private TaskCompletionSource<bool> _writeCompletionSource = null;

        private Action<Guid, byte[]> _onCharacteristicChanged = null;
        private Action<IBluetoothLEDevice> _onDeviceDisconnected = null;

        public BluetoothLEDevice(Context context, BluetoothAdapter bluetoothAdapter, string address)
        {
            _context = context;
            _bluetoothAdapter = bluetoothAdapter;
            Address = address;
        }

        public string Address { get; }
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

        public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(
            bool autoConnect,
            Action<Guid, byte[]> onCharacteristicChanged,
            Action<IBluetoothLEDevice> onDeviceDisconnected,
            CancellationToken token)
        {
            CancellationTokenRegistration tokenRegistration;

            lock(_lock)
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return null;
                }

                _onCharacteristicChanged = onCharacteristicChanged;
                _onDeviceDisconnected = onDeviceDisconnected;

                State = BluetoothLEDeviceState.Connecting;

                _bluetoothDevice = _bluetoothAdapter.GetRemoteDevice(Address);
                if (_bluetoothDevice == null)
                {
                    State = BluetoothLEDeviceState.Disconnected;
                    return null;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, autoConnect, this, BluetoothTransports.Le);
                }
                else
                {
                    _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, autoConnect, this);
                }

                if (_bluetoothGatt == null)
                {
                    _bluetoothDevice.Dispose();
                    _bluetoothDevice = null;
                    State = BluetoothLEDeviceState.Disconnected;
                    return null;
                }

                _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
                tokenRegistration = token.Register(() =>
                {
                    lock(_lock)
                    {
                        Disconnect();
                        _connectCompletionSource?.SetResult(null);
                    }
                });
            }

            var result = await _connectCompletionSource.Task;

            lock (_lock)
            {
                _connectCompletionSource = null;
                tokenRegistration.Dispose();
                return result;
            }
        }

        public void Disconnect()
        {
            lock(_lock)
            {
                _onDeviceDisconnected = null;
                _onCharacteristicChanged = null;

                if (_bluetoothGatt != null)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Close();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;

                    _bluetoothDevice.Dispose();
                    _bluetoothDevice = null;
                }

                State = BluetoothLEDeviceState.Disconnected;
            }
        }

        public bool EnableNotification(IGattCharacteristic characteristic)
        {
            lock (_lock)
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                if (!_bluetoothGatt.SetCharacteristicNotification(nativeCharacteristic, true))
                {
                    return false;
                }

                var descriptor = nativeCharacteristic.GetDescriptor(ClientCharacteristicConfigurationUUID);
                if (descriptor == null)
                {
                    return false;
                }

                if (!descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray()))
                {
                    return false;
                }

                return _bluetoothGatt.WriteDescriptor(descriptor);
            }
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
        {
            CancellationTokenRegistration tokenRegistration;

            lock (_lock)
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                nativeCharacteristic.WriteType = GattWriteType.Default;

                if (!nativeCharacteristic.SetValue(data))
                {
                    return false;
                }

                _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                if (!_bluetoothGatt.WriteCharacteristic(nativeCharacteristic))
                {
                    _writeCompletionSource = null;
                    return false;
                }

                tokenRegistration = token.Register(() =>
                {
                    lock (_lock)
                    {
                        _writeCompletionSource?.SetResult(false);
                    }
                });
            }

            var result = await _writeCompletionSource.Task;

            lock (_lock)
            {
                _writeCompletionSource = null;
                tokenRegistration.Dispose();
                return result;
            }
        }

        public bool WriteNoResponse(IGattCharacteristic characteristic, byte[] data)
        {
            lock (_lock)
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                nativeCharacteristic.WriteType = GattWriteType.NoResponse;

                if (!nativeCharacteristic.SetValue(data))
                {
                    return false;
                }

                if (!_bluetoothGatt.WriteCharacteristic(nativeCharacteristic))
                {
                    return false;
                }

                return true;
            }
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            System.Diagnostics.Debug.WriteLine($"OnConnectionStateChanged - status: {status}, newState: {newState}");

            switch (newState)
            {
                case ProfileState.Connecting:
                    break;

                case ProfileState.Connected:
                    lock (_lock)
                    {
                        if (State == BluetoothLEDeviceState.Connecting && status == GattStatus.Success)
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
                            Disconnect();
                            _connectCompletionSource?.SetResult(null);
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

                                // Copy the _onDeviceDisconnected callback to call it
                                // in case of an unexpected disconnection
                                var onDeviceDisconnected = _onDeviceDisconnected;

                                Disconnect();
                                onDeviceDisconnected?.Invoke(this);
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
            System.Diagnostics.Debug.WriteLine($"OnServiceDiscovered - status: {status}");

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

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            lock (_lock)
            {
                var guid = characteristic.Uuid.ToGuid();
                var data = characteristic.GetValue();
                _onCharacteristicChanged?.Invoke(guid, data);
            }
        }
    }
}