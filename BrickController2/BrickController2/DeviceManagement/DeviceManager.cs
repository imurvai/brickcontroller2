using BrickController2.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IBluetoothDeviceManager _bluetoothDeviceManager;
        private readonly IInfraredDeviceManager _infraredDeviceManager;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public DeviceManager(IBluetoothDeviceManager bluetoothDeviceManager, IInfraredDeviceManager infraredDeviceManager)
        {
            _bluetoothDeviceManager = bluetoothDeviceManager;
            _infraredDeviceManager = infraredDeviceManager;
        }

        public async Task ScanAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                var infraScan = _infraredDeviceManager.ScanAsync(FoundDevice, token);
                var bluetoothScan = _bluetoothDeviceManager.ScanAsync(FoundDevice, token);

                await Task.WhenAll(infraScan, bluetoothScan);
            }
        }

        private void FoundDevice(Device device)
        {
            // TODO: store device here
        }
    }
}
