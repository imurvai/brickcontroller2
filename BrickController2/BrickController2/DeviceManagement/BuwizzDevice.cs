using Plugin.BLE.Abstractions.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizzDevice : BluetoothDevice
    {
        public BuWizzDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 3;
        public override int DefaultOutputLevel => 2;

        public override Task SetOutputAsync(int channel, int value)
        {
            throw new System.NotImplementedException();
        }

        public override Task SetOutputLevelAsync(int value)
        {
            throw new System.NotImplementedException();
        }
    }
}
