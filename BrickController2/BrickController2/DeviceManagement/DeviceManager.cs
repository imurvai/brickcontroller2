using BrickController2.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IBluetoothDeviceManager _bluetoothDeviceManager;
        private readonly IInfraredDeviceManager _infraredDeviceManager;
        private readonly IDeviceRepository _deviceRepository;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public DeviceManager(
            IBluetoothDeviceManager bluetoothDeviceManager,
            IInfraredDeviceManager infraredDeviceManager,
            IDeviceRepository deviceRepository)
        {
            _bluetoothDeviceManager = bluetoothDeviceManager;
            _infraredDeviceManager = infraredDeviceManager;
            _deviceRepository = deviceRepository;
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public async Task LoadDevicesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                Devices.Clear();

                var deviceDTOs = await _deviceRepository.GetDevicesAsync();
                foreach (var deviceDTO in deviceDTOs)
                {
                    var device = CreateDevice(deviceDTO.DeviceType, deviceDTO.Name, deviceDTO.Address, deviceDTO.DeviceSpecificData);
                    if (device != null)
                    {
                        Devices.Add(device);
                    }
                }
            }
        }

        public async Task ScanAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                var infraScan = _infraredDeviceManager.ScanAsync(FoundDevice, token);
                var bluetoothScan = _bluetoothDeviceManager.ScanAsync(FoundDevice, token);

                await Task.WhenAll(infraScan, bluetoothScan);
            }

            async Task FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress)
            {
                if (Devices.Any(d => d.DeviceType == deviceType && d.Address == deviceAddress))
                {
                    return;
                }

                var device = CreateDevice(deviceType, deviceName, deviceAddress, null);
                if (device != null)
                {
                    await _deviceRepository.InsertDeviceAsync(device.DeviceType, device.Name, device.Address, device.DeviceSpecificData);
                    Devices.Add(device);
                }
            }
        }

        public async Task RenameDeviceAsync(Device device, string newName)
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.UpdateDeviceAsync(device.DeviceType, device.Address, newName);
                device.Name = newName;
            }
        }

        public async Task DeleteDeviceAsync(Device device)
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.DeleteDeviceAsync(device.DeviceType, device.Address);
            }
        }

        private Device CreateDevice(DeviceType deviceType, string deviceName, string deviceAddress, string deviceSpecificData)
        {
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                case DeviceType.SBrick:
                    return _bluetoothDeviceManager.CreateDevice(deviceType, deviceName, deviceAddress, deviceSpecificData);

                case DeviceType.Infrared:
                    return _infraredDeviceManager.CreateDevice(deviceType, deviceName, deviceAddress, deviceSpecificData);

                default:
                    return null;
            }
        }
    }
}
