using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleService
    {
        bool IsBleSupported { get; }
        bool IsBluetoothOn { get; }

        Task ScanAsync(Func<IBleDevice, Task> foundDeviceAsync, CancellationToken token);
    }
}
