using BrickController2.UI.Services.UIThread;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class InfraredDevice : Device
    {
        private readonly IInfraredDeviceManager _infraredDeviceManager;

        public InfraredDevice(string name, string address, IInfraredDeviceManager infraredDeviceManager, IUIThreadService uiThreadService, IDeviceRepository deviceRepository)
            : base(name, address, deviceRepository, uiThreadService)
        {
            _infraredDeviceManager = infraredDeviceManager;
        }

        public override DeviceType DeviceType => DeviceType.Infrared;
        public override int NumberOfChannels => 2;

        public override async Task<DeviceConnectionResult> ConnectAsync(bool reconnect, CancellationToken token)
        {
            await SetStateAsync(DeviceState.Connecting, false);

            var result = await _infraredDeviceManager.ConnectDevice(this);

            await SetStateAsync(result == DeviceConnectionResult.Ok ? DeviceState.Connected : DeviceState.Disconnected, result == DeviceConnectionResult.Error);
            return result;
        }

        public override async Task DisconnectAsync()
        {
            await SetStateAsync(DeviceState.Disconnecting, false);

            await _infraredDeviceManager.DisconnectDevice(this);

            await SetStateAsync(DeviceState.Disconnected, false);
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
