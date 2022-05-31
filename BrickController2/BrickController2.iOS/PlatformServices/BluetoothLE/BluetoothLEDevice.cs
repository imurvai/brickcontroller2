using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;
using Foundation;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : CBPeripheralDelegate, IBluetoothLEDevice
    {
        private readonly CBCentralManager _centralManager;
        private readonly CBPeripheral _peripheral;
        private readonly object _lock = new object();

        private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
        private TaskCompletionSource<IEnumerable<IGattCharacteristic>> _discoverCompletionSource = null;
        private TaskCompletionSource<byte[]> _readCompletionSource = null;
        private TaskCompletionSource<bool> _writeCompletionSource = null;

        private Action<Guid, byte[]> _onCharacteristicChanged = null;
        private Action<IBluetoothLEDevice> _onDeviceDisconnected = null;

        public BluetoothLEDevice(CBCentralManager centralManager, CBPeripheral peripheral)
        {
            _peripheral = peripheral;
            _peripheral.Delegate = this;
            _centralManager = centralManager;
        }

        public string Address => _peripheral.Identifier.ToString();
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

        public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(
            bool autoConnect,
            Action<Guid, byte[]> onCharacteristicChanged,
            Action<IBluetoothLEDevice> onDeviceDisconnected,
            CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    Disconnect();
                    _connectCompletionSource?.TrySetResult(null);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Disconnected)
                    {
                        return null;
                    }

                    _onCharacteristicChanged = onCharacteristicChanged;
                    _onDeviceDisconnected = onDeviceDisconnected;

                    State = BluetoothLEDeviceState.Connecting;
                    _centralManager.ConnectPeripheral(_peripheral, new PeripheralConnectionOptions { NotifyOnConnection = true, NotifyOnDisconnection = true });

                    _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
                }

                var result = await _connectCompletionSource.Task;

                lock (_lock)
                {
                    _connectCompletionSource = null;
                    return result;
                }
            }
        }

        internal void Disconnect()
        {
            _onDeviceDisconnected = null;
            _onCharacteristicChanged = null;

            _centralManager.CancelPeripheralConnection(_peripheral);
            State = BluetoothLEDeviceState.Disconnected;
        }

        public Task DisconnectAsync()
        {
            lock (_lock)
            {
                Disconnect();
            }

            return Task.CompletedTask;
        }

        public Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
        {
            lock(_lock)
            {
                var nativeCharacteristic = ((GattCharacteristic)characteristic).Characteristic;
                _peripheral.SetNotifyValue(true, nativeCharacteristic);
                return Task.FromResult(true);
            }
        }

        public async Task<byte[]> ReadAsync(IGattCharacteristic characteristic, CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    _readCompletionSource.TrySetResult(null);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Connected)
                    {
                        return null;
                    }

                    var nativeCharacteristic = ((GattCharacteristic)characteristic).Characteristic;

                    _readCompletionSource = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

                    _peripheral.ReadValue(nativeCharacteristic);
                }

                var result = await _readCompletionSource.Task;

                lock (_lock)
                {
                    _readCompletionSource = null;
                    return result;
                }
            }
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    _writeCompletionSource.TrySetResult(false);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Connected)
                    {
                        return false;
                    }

                    var nativeCharacteristic = ((GattCharacteristic)characteristic).Characteristic;
                    var nativeData = NSData.FromArray(data);

                    _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                    _peripheral.WriteValue(nativeData, nativeCharacteristic, CBCharacteristicWriteType.WithResponse);
                }

                var result = await _writeCompletionSource.Task;

                lock (_lock)
                {
                    _writeCompletionSource = null;
                    return result;
                }
            }
        }

        public Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
        {
            lock (_lock)
            {
                if (State != BluetoothLEDeviceState.Connected)
                {
                    return Task.FromResult(false);
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).Characteristic;
                var nativeData = NSData.FromArray(data);

                _peripheral.WriteValue(nativeData, nativeCharacteristic, CBCharacteristicWriteType.WithoutResponse);
                return Task.FromResult(true);
            }
        }

        public override async void DiscoveredService(CBPeripheral peripheral, NSError error)
        {
            try
            {
                if (error == null)
                {
                    var services = new List<GattService>();
                    if (_peripheral.Services != null)
                    {
                        foreach (var service in _peripheral.Services)
                        {
                            _discoverCompletionSource = new TaskCompletionSource<IEnumerable<IGattCharacteristic>>(TaskCreationOptions.RunContinuationsAsynchronously);

                            _peripheral.DiscoverCharacteristics(service);

                            var result = await _discoverCompletionSource.Task;
                            _discoverCompletionSource = null;

                            if (result != null)
                            {
                                services.Add(new GattService(service, result));
                            }
                            else
                            {
                                lock (_lock)
                                {
                                    Disconnect();
                                    _connectCompletionSource?.TrySetResult(null);
                                }
                                return;
                            }
                        }
                    }

                    lock (_lock)
                    {
                        State = BluetoothLEDeviceState.Connected;
                        _connectCompletionSource?.TrySetResult(services);
                        return;
                    }
                }
                else
                {
                    lock(_lock)
                    {
                        Disconnect();
                        _connectCompletionSource?.TrySetResult(null);
                    }
                }
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    Disconnect();
                    _connectCompletionSource?.TrySetResult(null);
                }
            }
        }

        public override void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
        {
            lock (_lock)
            {
                try
                {
                    if (error == null)
                    {
                        var characteristics = new List<GattCharacteristic>();
                        if (service.Characteristics != null)
                        {
                            foreach (var characteristic in service.Characteristics)
                            {
                                characteristics.Add(new GattCharacteristic(characteristic));
                            }
                        }

                        _discoverCompletionSource?.TrySetResult(characteristics);
                    }
                    else
                    {
                        _discoverCompletionSource?.TrySetResult(null);
                    }
                }
                catch (Exception)
                {
                    _discoverCompletionSource?.TrySetResult(null);
                }
            }
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            lock(_lock)
            {
                var data = error == null ? characteristic.Value?.ToArray() : null;

                if (_readCompletionSource != null)
                {
                    _readCompletionSource.TrySetResult(data);
                }
                else
                {
                    if (error == null)
                    {
                        var guid = characteristic.UUID.ToGuid();
                        _onCharacteristicChanged?.Invoke(guid, data);
                    }
                }
            }
        }

        public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            lock(_lock)
            {
                _writeCompletionSource?.TrySetResult(error == null);
            }
        }

        internal void OnDeviceConnected()
        {
            lock (_lock)
            {
                if (State == BluetoothLEDeviceState.Connecting)
                {
                    State = BluetoothLEDeviceState.Discovering;
                    Task.Run(() =>
                    {
                        Thread.Sleep(750);

                        lock (_lock)
                        {
                            if (State == BluetoothLEDeviceState.Discovering)
                            {
                                _peripheral.DiscoverServices();
                            }
                        }
                    });
                }
            }
        }

        internal void OnDeviceDisconnected()
        {
            lock (_lock)
            {
                switch (State)
                {
                    case BluetoothLEDeviceState.Connecting:
                    case BluetoothLEDeviceState.Discovering:
                        Disconnect();
                        _connectCompletionSource?.TrySetResult(null);
                        break;

                    case BluetoothLEDeviceState.Connected:
                        _writeCompletionSource?.TrySetResult(false);

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
        }
    }
}