using Plugin.BLE.Abstractions.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        public SBrickDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;

        public override Task SetOutputAsync(int channel, int value)
        {
            throw new System.NotImplementedException();
        }
    }
}
