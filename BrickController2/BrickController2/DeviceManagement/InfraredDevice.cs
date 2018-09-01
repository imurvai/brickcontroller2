using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class InfraredDevice : Device
    {
        private readonly IInfraredDeviceManager _infraredDeviceManager;

        public InfraredDevice(string name, string address, IInfraredDeviceManager infraredDeviceManager)
            : base(name, address)
        {
            _infraredDeviceManager = infraredDeviceManager;
        }

        public override DeviceType DeviceType => DeviceType.Infrared;
        public override int NumberOfChannels => 2;

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
