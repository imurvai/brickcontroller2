using BrickController2.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public abstract class Device : NotifyPropertyChangedSource
    {
        private string _name;

        protected Device(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public string Id => $"{DeviceType}#{Address}";

        public string Name
        {
            get => _name;
            set { _name = value; RaisePropertyChanged(); }
        }

        public string Address { get; }
        public string DeviceSpecificData => string.Empty;

        public abstract DeviceType DeviceType { get; }
        public abstract int NumberOfChannels { get; }
        public abstract int NumberOfOutputLevels { get; }
        public abstract int DefaultOutputLevel { get; }

        public abstract Task ConnectAsync(CancellationToken token);
        public abstract Task DisconnectAsync(CancellationToken token);

        public abstract Task SetOutputAsync(int channel, int value);
        public abstract Task SetOutputLevelAsync(int value);
    }
}
