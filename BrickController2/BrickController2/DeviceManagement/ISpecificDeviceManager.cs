using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal interface ISpecificDeviceManager
    {
        Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token);
    }
}
