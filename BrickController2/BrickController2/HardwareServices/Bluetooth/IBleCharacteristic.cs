using System;
using System.Threading.Tasks;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleCharacteristic
    {
        Guid Id { get; }

        Task ReadAsync();
        Task WriteAsync();
    }
}
