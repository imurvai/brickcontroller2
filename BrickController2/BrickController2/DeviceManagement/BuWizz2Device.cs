using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class BuWizz2Device : BluetoothDevice
    {
        public BuWizz2Device(string name, string address)
            : base(name, address)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz2;

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
