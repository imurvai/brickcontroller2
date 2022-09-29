using System;
using System.Text;

namespace BrickController2.Helpers
{
    public static class ByteArrayExtensions
    {
        public static string ToAsciiStringSafe(this byte[] data)
        {
            try
            {
                if (data != null)
                {
                    return Encoding.ASCII.GetString(data);
                }
            }
            catch
            {
            }

            return null;
        }

        public static Guid GetGuid(this byte[] data, int offset = 0)
        {
            return new Guid(data.GetInt32(offset + 12),
                data.GetInt16(offset + 10),
                data.GetInt16(offset + 8),
                data[offset + 7], data[offset + 6], data[offset + 5], data[offset + 4], data[offset + 3], data[offset + 2], data[offset + 1], data[offset]);
        }

        public static float GetFloat(this byte[] data, int offset = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(data, offset);
            }

            var reversedData = new byte[] { data[offset + 3], data[offset + 2], data[offset + 1], data[offset] };
            return BitConverter.ToSingle(reversedData, 0);
        }

        public static short GetInt16(this byte[] data, int offset)
        {
            return (short)(data[offset] |
                (data[offset + 1] << 8));
        }

        public static int GetInt32(this byte[] data, int offset)
        {
            return data[offset] |
                (data[offset + 1] << 8) |
                (data[offset + 2] << 16) |
                (data[offset + 3] << 24);
        }

        public static void SetInt32(this byte[] data, int offset, int value)
        {
            data[offset + 0] = (byte)(value & 0xff);
            data[offset + 1] = (byte)((value >> 8) & 0xff);
            data[offset + 2] = (byte)((value >> 16) & 0xff);
            data[offset + 3] = (byte)((value >> 24) & 0xff);
        }
    }
}
