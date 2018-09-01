using BrickController2.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public abstract class Device : NotifyPropertyChangedSource
    {
        private string _name;
        protected DeviceState _deviceState;
        protected int _output;
        protected int _outputLevel;

        protected Device(string name, string address)
        {
            _name = name;
            Address = address;
            _deviceState = DeviceState.Disconnected;
            _output = 0;
            _outputLevel = DefaultOutputLevel;
        }

        public abstract DeviceType DeviceType { get; }
        public string Address { get; }
        public string Id => $"{DeviceType}#{Address}";

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        public DeviceState DeviceState => _deviceState;
        public int Output => _output;
        public int OutputLevel => _outputLevel;

        public event EventHandler<DeviceStateChangedEventArgs> DeviceStateChanged;

        public abstract int NumberOfChannels { get; }
        public virtual int NumberOfOutputLevels => 1;
        public virtual int DefaultOutputLevel => 1;

        public abstract Task ConnectAsync(CancellationToken token);
        public abstract Task DisconnectAsync(CancellationToken token);

        public abstract Task SetOutputAsync(int channel, int value);
        public abstract Task SetOutputLevelAsync(int value);
    }
}
