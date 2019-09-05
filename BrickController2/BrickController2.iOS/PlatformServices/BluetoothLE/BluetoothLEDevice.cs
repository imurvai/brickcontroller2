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
            _onCharacteristicChanged = onCharacteristicChanged;
            _onDeviceDisconnected = onDeviceDisconnected;

            CancellationTokenRegistration tokenRegistration;

            lock (_lock)
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return null;
                }

                State = BluetoothLEDeviceState.Connecting;
                _centralManager.ConnectPeripheral(_peripheral, new PeripheralConnectionOptions { NotifyOnConnection = true, NotifyOnDisconnection = true });

                _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
                tokenRegistration = token.Register(() =>
                {
                    lock (_lock)
                    {
                        DisconnectInternal();
                        _connectCompletionSource?.SetResult(null);
                    }
                });
            }

            var result = await _connectCompletionSource.Task;
            _connectCompletionSource = null;
            tokenRegistration.Dispose();
            return result;
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                DisconnectInternal();
            }
        }

        private void DisconnectInternal()
        {
            _centralManager.CancelPeripheralConnection(_peripheral);
            State = BluetoothLEDeviceState.Disconnected;
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data)
        {
            lock(_lock)
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
            _writeCompletionSource = null;
            return result;
        }

        public bool WriteNoResponse(IGattCharacteristic characteristic, byte[] data)
        {
            lock (_lock)
            {
                if (State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).Characteristic;
                var nativeData = NSData.FromArray(data);

                _peripheral.WriteValue(nativeData, nativeCharacteristic, CBCharacteristicWriteType.WithoutResponse);
                return true;
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
                                    DisconnectInternal();
                                    _connectCompletionSource?.SetResult(null);
                                }
                                return;
                            }
                        }
                    }

                    lock (_lock)
                    {
                        State = BluetoothLEDeviceState.Connected;
                        _connectCompletionSource?.SetResult(services);
                        return;
                    }
                }
                else
                {
                    lock(_lock)
                    {
                        DisconnectInternal();
                        _connectCompletionSource?.SetResult(null);
                    }
                }
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    DisconnectInternal();
                    _connectCompletionSource?.SetResult(null);
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

                        _discoverCompletionSource?.SetResult(characteristics);
                    }
                    else
                    {
                        _discoverCompletionSource?.SetResult(null);
                    }
                }
                catch (Exception)
                {
                    _discoverCompletionSource?.SetResult(null);
                }
            }
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            lock(_lock)
            {
                if (error == null)
                {
                    var guid = characteristic.UUID.ToGuid();
                    var data = characteristic.Value?.ToArray();
                    _onCharacteristicChanged?.Invoke(guid, data);
                }
            }
        }

        public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            lock(_lock)
            {
                _writeCompletionSource?.SetResult(error == null);
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
                        DisconnectInternal();
                        _connectCompletionSource?.SetResult(null);
                        break;

                    case BluetoothLEDeviceState.Connected:
                        _writeCompletionSource?.SetResult(false);
                        DisconnectInternal();
                        _onDeviceDisconnected?.Invoke(this);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}