using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class SBrickDevice : BluetoothDevice
    {
        public SBrickDevice(string name, string address)
            : base(name, address)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 1;
        public override int DefaultOutputLevel => 1;

        public override Task ConnectAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override Task DisconnectAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override Task SetOutputAsync(int channel, int value)
        {
            throw new System.NotImplementedException();
        }

        public async override Task SetOutputLevelAsync(int value)
        {
            return;
        }
    }
}
