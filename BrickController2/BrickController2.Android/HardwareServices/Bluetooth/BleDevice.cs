using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using BrickController2.HardwareServices.Bluetooth;
using BrickController2.Helpers;

namespace BrickController2.Droid.HardwareServices.Bluetooth
{
    public class BleDevice : BluetoothGattCallback, IBleDevice, IDisposable
    {
        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly BluetoothDevice _bluetoothDevice;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        private BluetoothGatt _bluetoothGatt;

        private TaskCompletionSource<bool> _deviceConnectedTcs;
        private TaskCompletionSource<bool> _deviceDisconnectedTcs;
        private TaskCompletionSource<IEnumerable<Guid>> _servicesDiscoveredTcs;

        public BleDevice(Context context, BluetoothAdapter bluetoothAdapter, string address)
        {
            _context = context;
            _bluetoothAdapter = bluetoothAdapter;
            _bluetoothDevice = _bluetoothAdapter.GetRemoteDevice(address);
            Address = address;
            State = BleDeviceState.Disconnected;
        }

        public string Address { get; }
        public BleDeviceState State { get; private set; }

        public event EventHandler<BleDeviceStateChangedEventArgs> DeviceStateChanged;

        public async Task<bool> ConnectAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (_bluetoothGatt != null)
                {
                    return false;
                }

                try
                {
                    _deviceConnectedTcs = new TaskCompletionSource<bool>();
                    token.Register(() => _deviceConnectedTcs.SetCanceled());

                    _bluetoothGatt = _bluetoothDevice.ConnectGatt(_context, true, this);

                    return await _deviceConnectedTcs.Task;
                }
                catch (Exception)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;
                }
                finally
                {
                    _deviceConnectedTcs = null;
                }

                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (_bluetoothGatt != null)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;
                }
            }
        }

        public async Task<IEnumerable<Guid>> DiscoverServicesAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (_bluetoothGatt == null)
                {
                    return new List<Guid>();
                }

                try
                {
                    _servicesDiscoveredTcs = new TaskCompletionSource<IEnumerable<Guid>>();
                    token.Register(() => _servicesDiscoveredTcs.SetCanceled());
                    _bluetoothGatt.DiscoverServices();

                    return await _servicesDiscoveredTcs.Task;
                }
                catch (Exception)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;
                }
                finally
                {
                    _servicesDiscoveredTcs = null;
                }

                return null;
            }
        }

        #region GattCallback

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            if (gatt != _bluetoothGatt)
            {
                return;
            }

            var newBleState = BleDeviceState.Disconnected;
            switch (newState)
            {
                case ProfileState.Connected:
                    newBleState = BleDeviceState.Connected;
                    _deviceConnectedTcs?.SetResult(status == GattStatus.Success);
                    break;

                case ProfileState.Connecting:
                    newBleState = BleDeviceState.Connecting;
                    break;

                case ProfileState.Disconnected:
                    newBleState = BleDeviceState.Disconnected;
                    _deviceDisconnectedTcs?.SetResult(status == GattStatus.Success);
                    break;

                case ProfileState.Disconnecting:
                    newBleState = BleDeviceState.Disconnecting;
                    break;
            }

            SetState(newBleState, status == GattStatus.Success);
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            if (gatt != _bluetoothGatt)
            {
                return;
            }

        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            if (gatt != _bluetoothGatt)
            {
                return;
            }

        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            if (gatt != _bluetoothGatt)
            {
                return;
            }

        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            if (gatt != _bluetoothGatt)
            {
                return;
            }

        }

        #endregion

        private void SetState(BleDeviceState newState, bool isOk)
        {
            var oldState = State;
            State = newState;
            DeviceStateChanged?.Invoke(this, new BleDeviceStateChangedEventArgs(oldState, newState, isOk));
        }
    }
}