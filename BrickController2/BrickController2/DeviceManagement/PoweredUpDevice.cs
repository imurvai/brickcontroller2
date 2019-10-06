using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override int NumberOfChannels => 2;

        protected override void RegisterDefaultPorts()
        {
            RegisterPorts(new[]
            {
                new DevicePort(0, "1"),
                new DevicePort(1, "2"),
            });
        }
    }
}
