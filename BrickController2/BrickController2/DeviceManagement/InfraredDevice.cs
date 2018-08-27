using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class InfraredDevice : Device
    {
        public InfraredDevice(string name, string address)
            : base(name, address)
        {
        }

        public override DeviceType DeviceType => DeviceType.Infrared;
        public override int NumberOfChannels => 2;
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
