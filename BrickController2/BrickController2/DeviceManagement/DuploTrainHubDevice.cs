using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class DuploTrainHubDevice : ControlPlusDevice
    {
        public DuploTrainHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.DuploTrainHub;
        public override int NumberOfChannels => 1;
    }
}
