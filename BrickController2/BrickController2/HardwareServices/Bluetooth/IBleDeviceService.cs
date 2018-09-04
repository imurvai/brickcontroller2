using System;
using System.Threading.Tasks;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleDeviceService
    {
        Guid Id { get; }

        Task<IBleCharacteristic> GetCharacteristicAsync(Guid characteristicId);
    }
}
