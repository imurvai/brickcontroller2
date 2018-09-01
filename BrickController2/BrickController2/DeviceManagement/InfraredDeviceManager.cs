using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices;

namespace BrickController2.DeviceManagement
{
    internal class InfraredDeviceManager : IInfraredDeviceManager
    {
        private const int IR_FREQUENCY = 38000;
        private readonly IInfraredService _infraredService;

        public InfraredDeviceManager(IInfraredService infraredService)
        {
            _infraredService = infraredService;
        }

        public async Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token)
        {
            if (_infraredService.IsInfraredSupported && _infraredService.IsCarrierFrequencySupported(IR_FREQUENCY))
            {
                await deviceFoundCallback(DeviceType.Infrared, "PF Infra red 1", "Infrared-1");
                await deviceFoundCallback(DeviceType.Infrared, "PF Infra red 2", "Infrared-2");
                await deviceFoundCallback(DeviceType.Infrared, "PF Infra red 3", "Infrared-3");
                await deviceFoundCallback(DeviceType.Infrared, "PF Infra red 4", "Infrared-4");
            }
        }
    }
}
