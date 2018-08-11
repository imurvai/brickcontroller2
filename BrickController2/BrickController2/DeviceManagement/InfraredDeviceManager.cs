using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices;

namespace BrickController2.DeviceManagement
{
    public class InfraredDeviceManager : IInfraredDeviceManager
    {
        private readonly IInfraredService _infraredService;

        public InfraredDeviceManager(IInfraredService infraredService)
        {
            _infraredService = infraredService;
        }

        public Task ScanAsync(CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}
