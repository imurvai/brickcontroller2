using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;
using Foundation;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : CBPeripheralDelegate, IBluetoothLEDevice
    {
        private readonly CBCentralManager _centralManager;
        private readonly CBPeripheral _peripheral;
        private readonly AsyncLock _lock = new AsyncLock();

        private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
        private TaskCompletionSource<IEnumerable<IGattCharacteristic>> _discoverCompletionSource = null;
        private TaskCompletionSource<bool> _writeCompletionSource = null;

        public BluetoothLEDevice(CBCentralManager centralManager, CBPeripheral peripheral)
        {
            _peripheral = peripheral;
            _peripheral.Delegate = this;
            _centralManager = centralManager;
        }

        public string Address => _peripheral.UUID.ToString();
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
                _centralManager.ConnectPeripheral(_peripheral, new PeripheralConnectionOptions { NotifyOnConnection = true, NotifyOnDisconnection = true });

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
            _centralManager.CancelPeripheralConnection(_peripheral);
            State = BluetoothLEDeviceState.Disconnected;
        }

        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data)
        {
            using (await _lock.LockAsync())
            {
            }

            throw new NotImplementedException();
        }

        public async Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data)
        {
            using (await _lock.LockAsync())
            {
            }

            throw new NotImplementedException();
        }

        public override async void DiscoveredService(CBPeripheral peripheral, NSError error)
        {
            using (await _lock.LockAsync())
            {
                if (error == null)
                {
                    var services = new List<GattService>();
                    if (_peripheral.Services != null)
                    {
                        foreach (var service in _peripheral.Services)
                        {
                            _discoverCompletionSource = new TaskCompletionSource<IEnumerable<IGattCharacteristic>>();
                            _peripheral.DiscoverCharacteristics(service);

                            var result = await _discoverCompletionSource.Task;
                            _discoverCompletionSource = null;

                            if (result != null)
                            {
                                services.Add(new GattService(service, result));
                            }
                            else
                            {
                                DisconnectInternal();
                                _connectCompletionSource?.SetResult(null);
                                return;
                            }
                        }
                    }

                    _connectCompletionSource?.SetResult(services);
                }
                else
                {
                    DisconnectInternal();
                    _connectCompletionSource?.SetResult(null);
                }
            }
        }

        public override async void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
        {
            using (await _lock.LockAsync())
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
        }

        internal async void OnDeviceConnected()
        {
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
                if (State == BluetoothLEDeviceState.Disconnecting)
                {
                    _peripheral.DiscoverServices();
                }
            }
        }

        internal async void OnDeviceDisconnected()
        {
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
        }
    }
}