using System;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IGattCharacteristic
    {
        Guid Uuid { get; }
    }
}
