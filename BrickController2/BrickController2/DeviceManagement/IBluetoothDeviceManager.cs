using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IBluetoothDeviceManager
    {
        Task ScanAsync(Action<Device> deviceFoundCallback, CancellationToken token);
    }
}
