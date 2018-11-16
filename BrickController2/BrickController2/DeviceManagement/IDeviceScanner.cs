using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal interface IDeviceScanner
    {
        Task<bool> ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token);
    }
}
