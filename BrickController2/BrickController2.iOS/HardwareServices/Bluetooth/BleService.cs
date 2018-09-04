using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices.Bluetooth;

namespace BrickController2.iOS.HardwareServices.Bluetooth
{
    public class BleService : IBleService
    {
        public bool IsBleSupported => throw new NotImplementedException();

        public bool IsBluetoothOn => throw new NotImplementedException();

        public Task ScanAsync(Func<IBleDevice, Task> foundDeviceAsync, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}