using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        public TechnicHubDevice(
            string name, 
            string address, 
            byte[] deviceData, 
            IDeviceRepository deviceRepository, 
            IUIThreadService uiThreadService, 
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override int NumberOfChannels => 4;
    }
}
