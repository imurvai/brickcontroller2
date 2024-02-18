using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Windows.Extensions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

public class BleDevice : IBluetoothLEDevice
{
    private readonly AsyncLock _lock = new();

    private BluetoothLEDevice _bluetoothDevice;
    private ICollection<BleGattService> _services;

    private TaskCompletionSource<ICollection<BleGattService>> _connectCompletionSource;

    private Action<Guid, byte[]> _onCharacteristicChanged;
    private Action<IBluetoothLEDevice> _onDeviceDisconnected;

    public BleDevice(string address)
    {
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
        using (var tokenRegistration = token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                InternalDisconnect();
                _connectCompletionSource?.TrySetResult(null);
            }
        }))
        {
            _services = await ConnectAsync(onCharacteristicChanged, onDeviceDisconnected);
            return _services;
        }
    }

    private async Task<ICollection<BleGattService>> ConnectAsync(
        Action<Guid, byte[]> onCharacteristicChanged,
        Action<IBluetoothLEDevice> onDeviceDisconnected)
    {
        using (await _lock.LockAsync())
        {
            if (State != BluetoothLEDeviceState.Disconnected)
            {
                return null;
            }
            _onCharacteristicChanged = onCharacteristicChanged;
            _onDeviceDisconnected = onDeviceDisconnected;

            State = BluetoothLEDeviceState.Connecting;

            if (Address.TryParseBluetoothAddressString(out var bluetoothAddress))
            {
                _bluetoothDevice?.Dispose();
                _bluetoothDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
            }

            if (_bluetoothDevice == null)
            {
                InternalDisconnect();
                return null;
            }

            _bluetoothDevice.ConnectionStatusChanged += _bluetoothDevice_ConnectionStatusChanged;

            _connectCompletionSource = new TaskCompletionSource<ICollection<BleGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        // enforce connection check
        await OnConnection();

        var result = await _connectCompletionSource.Task;
        _connectCompletionSource = null;

        return result;
    }

    public async Task DisconnectAsync()
    {
        using (await _lock.LockAsync())
        {
            InternalDisconnect();
        }
    }

    private void InternalDisconnect()
    {
        _onDeviceDisconnected = null;
        _onCharacteristicChanged = null;

        if (_services != null)
        {
            foreach (var service in _services)
            {
                service.Dispose();
            }
            _services = null;
        }

        if (_bluetoothDevice != null)
        {
            _bluetoothDevice.ConnectionStatusChanged -= _bluetoothDevice_ConnectionStatusChanged;
            _bluetoothDevice.Dispose();
            _bluetoothDevice = null;
        }

        State = BluetoothLEDeviceState.Disconnected;
    }

    public async Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic &&
                bleGattCharacteristic.CanNotify)
            {
                return await bleGattCharacteristic
                    .EnableNotificationAsync(_onCharacteristicChanged);
            }

            return false;
        }
    }

    public async Task<bool> DisableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic &&
                bleGattCharacteristic.CanNotify)
            {
                return await bleGattCharacteristic.DisableNotificationAsync();
            }

            return false;
        }
    }

    public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {

                var result = await bleGattCharacteristic.WriteWithResponseAsync(data);
                return result.Status == GattCommunicationStatus.Success;
            }
            return false;
        }
    }

    public async Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {
                var result = await bleGattCharacteristic.WriteNoResponseAsync(data);
                return result == GattCommunicationStatus.Success;
            }
            return false;
        }
    }

    public async Task<byte[]> ReadAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {
                var result = await bleGattCharacteristic.ReadValueAsync();

                if (result.Status == GattCommunicationStatus.Success)
                {
                    return result.Value.ToByteArray();
                }
            }
            return null;
        }
    }

    private void _bluetoothDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        // check for a raise condition
        if (sender != _bluetoothDevice)
            return;

        // uses lock inside OnXXX methods, execution is not awaited
        switch (sender.ConnectionStatus)
        {
            case BluetoothConnectionStatus.Connected:
                _ = OnConnection();
                break;

            case BluetoothConnectionStatus.Disconnected:
                _ = OnDisconnection();
                break;
        }
    }

    private async Task OnConnection()
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connecting)
            {
                State = BluetoothLEDeviceState.Discovering;

                await DiscoverServices(BluetoothCacheMode.Uncached);
            }
            else if (State == BluetoothLEDeviceState.Connected)
            {
                // no need to react
            }
            else
            {
                InternalDisconnect();
                _connectCompletionSource?.SetResult(null);
            }
        }
    }

    private async Task OnDisconnection()
    {
        using (await _lock.LockAsync())
        {
            switch (State)
            {
                case BluetoothLEDeviceState.Connecting:
                case BluetoothLEDeviceState.Discovering:
                    InternalDisconnect();
                    _connectCompletionSource?.SetResult(null);
                    break;

                case BluetoothLEDeviceState.Connected:

                    var onDeviceDisconnected = _onDeviceDisconnected;
                    InternalDisconnect();
                    onDeviceDisconnected?.Invoke(this);
                    break;

                default:
                    break;
            }
        }
    }

    private async Task<bool> DiscoverServices(BluetoothCacheMode cacheMode)
    {
        // expectation is the method is already called within lock
        if (_bluetoothDevice != null && State == BluetoothLEDeviceState.Discovering)
        {
            var services = new List<BleGattService>();

            var availabelServices = await _bluetoothDevice.GetGattServicesAsync(cacheMode);

            if (availabelServices.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in availabelServices.Services)
                {
                    var openStatus = await service.OpenAsync(GattSharingMode.SharedReadAndWrite);

                    if (openStatus != GattOpenStatus.Success)
                    {
                        //TODO log
                        continue;
                    }

                    var availableCharacteristics = await service.GetCharacteristicsAsync(cacheMode);

                    if (availableCharacteristics.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = availableCharacteristics.Characteristics
                            .Select(ch => new BleGattCharacteristic(ch))
                            .ToList();

                        services.Add(new BleGattService(service, characteristics));
                    }
                }
                State = BluetoothLEDeviceState.Connected;
                _connectCompletionSource?.SetResult(services);
                return true;
            }
        }
        InternalDisconnect();
        _connectCompletionSource?.SetResult(null);
        return false;
    }
}