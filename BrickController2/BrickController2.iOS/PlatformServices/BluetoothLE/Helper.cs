using CoreBluetooth;
using Foundation;
using System;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    internal static class Helper
    {
        public static Guid ToGuid(this NSUuid uuid)
        {
            return Guid.ParseExact(uuid.AsString(), "d");
        }

        public static Guid ToGuid(this CBUUID uuid)
        {
            var id = uuid.ToString();
            if (id.Length == 4)
            {
                id = $"0000{id}-0000-1000-8000-00805f9b34fb";
            }

            return Guid.ParseExact(id, "d");
        }

        public static NSUuid ToNsUuid(this Guid guid)
        {
            return new NSUuid(guid.ToString());
        }
    }
}