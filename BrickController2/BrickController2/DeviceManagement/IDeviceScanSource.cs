using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceScanSource
    {
        Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token);
    }
}
