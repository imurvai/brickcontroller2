using BrickController2.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public abstract class Device : NotifyPropertyChangedSource
    {
        private readonly IDeviceRepository _deviceRepository;
        protected readonly AsyncLock _asyncLock = new AsyncLock();

        private string _name;
        private string _firmwareVersion = "-";
        private string _hardwareVersion = "-";
        private string _batteryVoltage = "-";

        private volatile DeviceState _deviceState;
        protected int _outputLevel;

        internal Device(string name, string address, IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;

            _name = name;
            Address = address;
            _deviceState = DeviceState.Disconnected;
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

        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            protected set { _firmwareVersion = value; RaisePropertyChanged(); }
        }

        public string HardwareVersion
        {
            get { return _hardwareVersion; }
            protected set { _hardwareVersion = value; RaisePropertyChanged(); }
        }

        public string BatteryVoltage
        {
            get { return _batteryVoltage; }
            protected set { _batteryVoltage = value; RaisePropertyChanged(); }
        }

        public virtual string BatteryVoltageSign => string.Empty;

        public DeviceState DeviceState
        {
            get { return _deviceState; }
            protected set { _deviceState = value; RaisePropertyChanged(); }
        }

        public int OutputLevel => _outputLevel;

        public abstract int NumberOfChannels { get; }
        public virtual int NumberOfOutputLevels => 1;
        public virtual int DefaultOutputLevel => 1;

        public virtual bool CanChangeOutputType(int channel) => false;

        public abstract Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token);
        public abstract Task DisconnectAsync();

        public abstract void SetOutput(int channel, float value);

        public virtual bool CanSetOutputLevel => false;
        public virtual void SetOutputLevel(int value) { }

        public virtual bool CanResetOutput(int channel) => false;
        public virtual Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public virtual bool CanAutoCalibrateOutput(int channel) => false;
        public virtual bool CanChangeMaxServoAngle(int channel) => false;
        public virtual Task<(bool Success, float BaseServoAngle)> AutoCalibrateOutputAsync(int channel, CancellationToken token)
        {
            return Task.FromResult((true, 0F));
        }

        public virtual bool CanBePowerSource => false;

        public async Task RenameDeviceAsync(Device device, string newName)
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.UpdateDeviceAsync(device.DeviceType, device.Address, newName);
                device.Name = newName;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        protected void CheckChannel(int channel)
        {
            if (channel < 0 || channel >= NumberOfChannels)
            {
                throw new ArgumentOutOfRangeException($"Invalid channel value: {channel}.");
            }
        }

        protected float CutOutputValue(float outputValue)
        {
            return Math.Max(-1F, Math.Min(1F, outputValue));
        }
    }
}
