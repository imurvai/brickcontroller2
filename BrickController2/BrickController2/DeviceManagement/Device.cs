using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public abstract class Device
    {
        public Device(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; }
        public string Address { get; }
        public string Id => $"{Name}-{Address}";

        public abstract DeviceType DeviceType { get; }
        public abstract int NumberOfChannels { get; }

        public abstract Task ConnectAsync(CancellationToken token);
        public abstract Task DisconnectAsync(CancellationToken token);

        public abstract Task SetOutputAsync(int channel, int value);
    }
}
