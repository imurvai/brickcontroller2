using System;
using Java.Util;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public static class Helper
    {
        public static Guid ToGuid(this UUID uuid)
        {
            return Guid.ParseExact(uuid.ToString(), "d");
        }

        public static UUID ToUUID(this Guid guid)
        {
            return UUID.FromString(guid.ToString())!;
        }
    }
}