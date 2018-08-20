using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices;

namespace BrickController2.DeviceManagement
{
    public class InfraredDeviceManager : IInfraredDeviceManager
    {
        private const int IR_FREQUENCY = 38000;
        private readonly IInfraredService _infraredService;

        public InfraredDeviceManager(IInfraredService infraredService)
        {
            _infraredService = infraredService;
        }

        public async Task ScanAsync(Func<Device, Task> deviceFoundCallback, CancellationToken token)
        {
            if (_infraredService.IsInfraredSupported && _infraredService.IsCarrierFrequencySupported(IR_FREQUENCY))
            {
                // TODO: call callback with the 4 PF device
            }
        }
    }
}
