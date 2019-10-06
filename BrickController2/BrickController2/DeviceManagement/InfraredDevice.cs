using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class InfraredDevice : Device
    {
        private readonly IInfraredDeviceManager _infraredDeviceManager;

        public InfraredDevice(string name, string address, byte[] deviceData, IInfraredDeviceManager infraredDeviceManager, IDeviceRepository deviceRepository)
            : base(name, address, deviceRepository)
        {
            _infraredDeviceManager = infraredDeviceManager;

            // setup default list of ports here as the list does not change
            RegisterPorts(
                new[]
                {
                    new DevicePort(0, "Blue"),
                    new DevicePort(1, "Red"),
                });
        }

        public override DeviceType DeviceType => DeviceType.Infrared;
        public override int NumberOfChannels => 2;

        public override async Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            DeviceState = DeviceState.Connecting;

            var result = await _infraredDeviceManager.ConnectDevice(this);

            DeviceState = result == DeviceConnectionResult.Ok ? DeviceState.Connected : DeviceState.Disconnected;
            return result;
        }

        public override async Task DisconnectAsync()
        {
            DeviceState = DeviceState.Disconnecting;

            await _infraredDeviceManager.DisconnectDevice(this);

            DeviceState = DeviceState.Disconnected;
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(7 * value);
            _infraredDeviceManager.SetOutput(this, channel, intValue);
        }
    }
}
