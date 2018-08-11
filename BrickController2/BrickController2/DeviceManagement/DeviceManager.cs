using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IBluetoothDeviceManager _bluetoothDeviceManager;
        private readonly IInfraredDeviceManager _infraredDeviceManager;

        public DeviceManager(IBluetoothDeviceManager bluetoothDeviceManager, IInfraredDeviceManager infraredDeviceManager)
        {
            _bluetoothDeviceManager = bluetoothDeviceManager;
            _infraredDeviceManager = infraredDeviceManager;
        }

        public async Task ScanAsync(CancellationToken token)
        {
            await _bluetoothDeviceManager.ScanAsync(token);
        }
    }
}
