using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : BluetoothGattCallback, IBluetoothLEDevice
    {
        private readonly Context _context;
        private readonly BluetoothDevice _bluetoothDevice;
        private readonly IDictionary<string, BluetoothGattCharacteristic> _characteristicMap = new Dictionary<string, BluetoothGattCharacteristic>();
        private readonly object _lock = new object();

        private BluetoothGatt _bluetoothGatt = null;
        private TaskCompletionSource<bool> _connectCompletionSource = null;

        public BluetoothLEDevice(Context context, BluetoothAdapter bluetoothAdapter, string address)
        {
            _context = context;
            _bluetoothDevice = bluetoothAdapter.GetRemoteDevice(address);
        }

        public string Address => _bluetoothDevice.Address;
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;
        public IDictionary<string, IEnumerable<string>> ServicesAndCharacteristics { get; } = new Dictionary<string, IEnumerable<string>>();

        public event EventHandler<BluetoothLEDeviceStateChangedEventArgs> StateChanged;

        public Task<bool> ConnectAndDiscoverServicesAsync(CancellationToken token)
        {
            lock (_lock)
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return Task.FromResult(false);
                }

                _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, true, this);
                if (_bluetoothGatt == null)
                {
                    return Task.FromResult(false);
                }

                _connectCompletionSource = new TaskCompletionSource<bool>();
                token.Register(() =>
                {
                    _connectCompletionSource.SetCanceled();
                    DisconnectInternal();
                });

                return _connectCompletionSource.Task;
            }
        }

        public Task DisconnectAsync()
        {
            lock (_lock)
            {
                DisconnectInternal();
                return Task.FromResult(true);
            }
        }

        public bool Write(string characteristic, byte[] data, bool noResponse = false)
        {
            lock (_lock)
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected || !_characteristicMap.ContainsKey(characteristic))
                {
                    return false;
                }

                var gattCharacteristic = _characteristicMap[characteristic];

                gattCharacteristic.WriteType = noResponse ? GattWriteType.NoResponse : GattWriteType.Default;
                if (!gattCharacteristic.SetValue(data))
                {
                    return false;
                }

                return _bluetoothGatt.WriteCharacteristic(gattCharacteristic);
            }
        }

        private void DisconnectInternal()
        {
            lock (_lock)
            {
                if (_bluetoothGatt != null)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;
                }

                _characteristicMap.Clear();
                ServicesAndCharacteristics.Clear();
            }
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            if (status != GattStatus.Success)
            {
                return;
            }

            var oldState = State;

            switch (newState)
            {
                case ProfileState.Connecting:
                    State = BluetoothLEDeviceState.Connecting;
                    break;

                case ProfileState.Connected:
                    _bluetoothGatt.DiscoverServices();
                    break;

                case ProfileState.Disconnecting:
                    State = BluetoothLEDeviceState.Disconnecting;
                    break;

                case ProfileState.Disconnected:
                    State = BluetoothLEDeviceState.Disconnected;
                    break;
            }

            StateChanged?.Invoke(this, new BluetoothLEDeviceStateChangedEventArgs(this, oldState, State));
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            if (status != GattStatus.Success)
            {
                return;
            }

            if (gatt.Services != null)
            {
                foreach (var service in gatt.Services)
                {
                    var serviceUuid = service.Uuid.ToString();
                    var characteristicUuidList = new List<string>();
                    ServicesAndCharacteristics[serviceUuid] = characteristicUuidList;

                    if (service.Characteristics != null)
                    {
                        foreach (var characteristic in service.Characteristics)
                        {
                            var characteristicUuid = characteristic.Uuid.ToString();
                            _characteristicMap[characteristicUuid] = characteristic;
                            characteristicUuidList.Add(characteristicUuid);
                        }
                    }
                }
            }

            var oldState = State;
            State = BluetoothLEDeviceState.Connected;
            StateChanged?.Invoke(this, new BluetoothLEDeviceStateChangedEventArgs(this, oldState, State));
            _connectCompletionSource?.SetResult(true);
        }
    }
}