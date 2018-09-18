using BrickController2.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class DeviceManager : NotifyPropertyChangedSource, IDeviceManager
    {
        private readonly IBluetoothDeviceManager _bluetoothDeviceManager;
        private readonly IInfraredDeviceManager _infraredDeviceManager;
        private readonly IDeviceRepository _deviceRepository;
        private readonly DeviceFactory _deviceFactory;
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly AsyncLock _foundDeviceLock = new AsyncLock();

        private bool _isScanning;

        public DeviceManager(
            IBluetoothDeviceManager bluetoothDeviceManager,
            IInfraredDeviceManager infraredDeviceManager,
            IDeviceRepository deviceRepository,
            DeviceFactory deviceFactory)
        {
            _bluetoothDeviceManager = bluetoothDeviceManager;
            _infraredDeviceManager = infraredDeviceManager;
            _deviceRepository = deviceRepository;
            _deviceFactory = deviceFactory;
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public bool IsScanning
        {
            get { return _isScanning; }
            set { _isScanning = value; RaisePropertyChanged(); }
        }

        public async Task LoadDevicesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                Devices.Clear();

                var deviceDTOs = await _deviceRepository.GetDevicesAsync();
                foreach (var deviceDTO in deviceDTOs)
                {
                    var device = _deviceFactory(deviceDTO.DeviceType, deviceDTO.Name, deviceDTO.Address);
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
                IsScanning = true;

                try
                {
                    var infraScan = _infraredDeviceManager.ScanAsync(FoundDevice, token);
                    var bluetoothScan = _bluetoothDeviceManager.ScanAsync(FoundDevice, token);

                    await Task.WhenAll(infraScan, bluetoothScan);
                }
                catch (Exception)
                {
                }

                IsScanning = false;
            }

            async Task FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress)
            {
                using (await _foundDeviceLock.LockAsync())
                {
                    if (Devices.Any(d => d.DeviceType == deviceType && d.Address == deviceAddress))
                    {
                        return;
                    }

                    var device = _deviceFactory(deviceType, deviceName, deviceAddress);
                    if (device != null)
                    {
                        await _deviceRepository.InsertDeviceAsync(device.DeviceType, device.Name, device.Address);
                        Devices.Add(device);
                    }
                }
            }
        }

        public Device GetDeviceById(string Id)
        {
            var deviceTypeAndAddress = Id.Split('#');
            var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), deviceTypeAndAddress[0]);
            var deviceAddress = deviceTypeAndAddress[1];
            return Devices.FirstOrDefault(d => d.DeviceType == deviceType && d.Address == deviceAddress);
        }

        public async Task DeleteDeviceAsync(Device device)
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.DeleteDeviceAsync(device.DeviceType, device.Address);
                Devices.Remove(device);
            }
        }

        public async Task DeleteDevicesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.DeleteDevicesAsync();
                Devices.Clear();
            }
        }
    }
}
