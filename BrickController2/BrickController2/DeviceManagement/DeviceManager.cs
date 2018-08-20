using BrickController2.Helpers;
using System;
using System.Collections.ObjectModel;
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
                    switch (deviceDTO.DeviceType)
                    {
                        case DeviceType.BuWizz:
                            break;

                        case DeviceType.BuWizz2:
                            break;

                        case DeviceType.SBrick:
                            break;

                        case DeviceType.Infrared:
                            // TODO: temp code
                            var device = new InfraredDevice(deviceDTO.Name, deviceDTO.Address);
                            Devices.Add(device);
                            break;

                        default:
                            throw new InvalidOperationException($"Not supported device type: {deviceDTO.DeviceType}.");
                    }
                }
            }
        }

        public async Task ScanAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                Devices.Clear();
                await _deviceRepository.DeleteDevicesAsync();

                var infraScan = _infraredDeviceManager.ScanAsync(FoundDevice, token);
                var bluetoothScan = _bluetoothDeviceManager.ScanAsync(FoundDevice, token);

                await Task.WhenAll(infraScan, bluetoothScan);
            }
        }

        private async Task FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress)
        {
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                    break;

                case DeviceType.BuWizz2:
                    break;

                case DeviceType.SBrick:
                    break;

                case DeviceType.Infrared:
                    // TODO: temp code
                    var device = new InfraredDevice(deviceName, deviceAddress);
                    await _deviceRepository.InsertDeviceAsync(new DeviceDTO { DeviceType = DeviceType.Infrared, Name = deviceName, Address = deviceAddress, DeviceSpecificData = null });
                    Devices.Add(device);
                    break;
            }
        }
    }
}
