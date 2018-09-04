using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleDevice
    {
        string Address { get; }
        BleDeviceState State { get; }

        event EventHandler<BleDeviceStateChangedEventArgs> DeviceStateChanged;

        Task<bool> ConnectAsync(CancellationToken token);
        Task DisconnectAsync();

        Task<IEnumerable<Guid>> DiscoverServicesAsync(CancellationToken token);
    }
}
