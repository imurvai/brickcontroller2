using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class InfraredDevice : Device
    {
        private readonly IInfraredDeviceManager _infraredDeviceManager;

        public InfraredDevice(string name, string address, IInfraredDeviceManager infraredDeviceManager, IDeviceRepository deviceRepository)
            : base(name, address, deviceRepository)
        {
            _infraredDeviceManager = infraredDeviceManager;
        }

        public override DeviceType DeviceType => DeviceType.Infrared;
        public override int NumberOfChannels => 2;

        public override async Task<DeviceConnectionResult> ConnectAsync(CancellationToken token)
        {
            SetState(DeviceState.Connecting, false);

            var result = await _infraredDeviceManager.ConnectDevice(this);

            SetState(result == DeviceConnectionResult.Ok ? DeviceState.Connected : DeviceState.Disconnected, result == DeviceConnectionResult.Error);
            return result;
        }

        public override async Task DisconnectAsync()
        {
            SetState(DeviceState.Disconnecting, false);

            await _infraredDeviceManager.DisconnectDevice(this);

            SetState(DeviceState.Disconnected, false);
        }

        public override async Task SetOutputAsync(int channel, int value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            await _infraredDeviceManager.SetOutput(this, channel, value);
        }
    }
}
