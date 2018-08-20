using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class BuWizzDevice : BluetoothDevice
    {
        public BuWizzDevice(string name, string address)
            : base(name, address)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;

        public override int NumberOfChannels => 4;

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
    }
}
