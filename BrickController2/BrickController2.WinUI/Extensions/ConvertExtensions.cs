namespace BrickController2.Windows.Extensions;

public static class ConvertExtensions
{
    public static string ToBluetoothAddressString(this ulong bluetoothAddress)
    {
        // 48bit physical BT address
        var a = (byte)((bluetoothAddress >> 40) & 0xFF);
        var b = (byte)((bluetoothAddress >> 32) & 0xFF);
        var c = (byte)((bluetoothAddress >> 24) & 0xFF);
        var d = (byte)((bluetoothAddress >> 16) & 0xFF);
        var e = (byte)((bluetoothAddress >> 8) & 0xFF);
        var f = (byte)(bluetoothAddress & 0xFF);

        return $"{a:X2}:{b:X2}:{c:X2}:{d:X2}:{e:X2}:{f:X2}";
    }

    public static bool TryParseBluetoothAddressString(this string stringValue, out ulong bluetoothAddress)
    {
        bluetoothAddress = default;

        if (string.IsNullOrEmpty(stringValue) || stringValue.Length != 17)
        {
            return false;
        }

        ulong value = 0;

        for (int i = 1; i <= stringValue.Length; i++)
        {
            var ch = (uint)stringValue[i - 1];
            if (i % 3 == 0)
            {
                if (ch != '-' && ch != ':')
                {
                    // missing dash
                    return false;
                }
            }
            else if (ch >= 0x30 && ch <= 0x39)
            {
                value = (value << 4) + ch - 0x30;
            }
            else if (ch >= 0x41 && ch <= 0x46)
            {
                value = (value << 4) + ch - 0x37;
            }
            else
            {
                // wrong character
                return false;
            }
        }

        bluetoothAddress = value;
        return true;
    }
}
