using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override int NumberOfChannels => 4;
    }
}
