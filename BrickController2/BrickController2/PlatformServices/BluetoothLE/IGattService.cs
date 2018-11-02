using System;
using System.Collections.Generic;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IGattService
    {
        Guid Uuid { get; }
        IEnumerable<IGattCharacteristic> Characteristics { get; }
    }
}
