using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IUIThreadService uiThreadService,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override int NumberOfChannels => 4;
    }
}
