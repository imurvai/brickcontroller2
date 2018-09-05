using System;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal interface ISpecificDeviceManager
    {
        Task StartScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback);
        Task StopScanAsync();
    }
}
