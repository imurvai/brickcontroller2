using System;
using System.Threading.Tasks;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleService
    {
        Guid Id { get; }

        Task<IBleCharacteristic> GetCharacteristicAsync(Guid characteristicId);
    }
}
