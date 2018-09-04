using System;
using System.Collections.Generic;

namespace BrickController2.HardwareServices.Bluetooth
{
    public interface IBleDeviceService
    {
        Guid Id { get; }

        IEnumerable<Guid> GetCharacteristicsAsync();
    }
}
