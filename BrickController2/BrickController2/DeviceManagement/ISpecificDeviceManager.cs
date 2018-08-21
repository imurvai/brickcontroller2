using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface ISpecificDeviceManager
    {
        Task ScanAsync(Func<DeviceType, string, string, Task> deviceFoundCallback, CancellationToken token);
        Device CreateDevice(DeviceType deviceType, string name, string address, string deviceSpecificData = null);
    }
}
