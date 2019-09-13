using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        public TechnicHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override int NumberOfChannels => 4;

    }
}
