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

            return string.Empty;
        }

        public static Guid GetGuid(this byte[] data, int offset = 0)
        {
            return new Guid(
                data.GetInt32(offset + 12),
                data.GetInt16(offset + 10),
                data.GetInt16(offset + 8),
                data[offset + 7], data[offset + 6], data[offset + 5], data[offset + 4], data[offset + 3], data[offset + 2], data[offset + 1], data[offset]);
        }

        public static short GetInt16(this byte[] data, int offset = 0)
        {
            var tempBuffer = BitConverter.IsLittleEndian ?
                new byte[] { data[offset + 0], data[offset + 1] } :
                new byte[] { data[offset + 1], data[offset + 0] };
            return BitConverter.ToInt16(tempBuffer, 0);
        }

        public static int GetInt32(this byte[] data, int offset = 0)
        {
            var tempBuffer = BitConverter.IsLittleEndian ?
                new byte[] { data[offset + 0], data[offset + 1], data[offset + 2], data[offset + 3] } :
                new byte[] { data[offset + 3], data[offset + 2], data[offset + 1], data[offset + 0] };
            return BitConverter.ToInt32(tempBuffer, 0);
        }

        public static void SetInt32(this byte[] data, int value, int offset = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                data[offset + 0] = (byte)(value & 0xff);
                data[offset + 1] = (byte)((value >> 8) & 0xff);
                data[offset + 2] = (byte)((value >> 16) & 0xff);
                data[offset + 3] = (byte)((value >> 24) & 0xff);
            }
            else
            {
                data[offset + 3] = (byte)(value & 0xff);
                data[offset + 2] = (byte)((value >> 8) & 0xff);
                data[offset + 1] = (byte)((value >> 16) & 0xff);
                data[offset + 0] = (byte)((value >> 24) & 0xff);
            }
        }

        public static float GetFloat(this byte[] data, int offset = 0)
        {
            var tempBuffer = BitConverter.IsLittleEndian ?
                new byte[] { data[offset + 0], data[offset + 1], data[offset + 2], data[offset + 3] } :
                new byte[] { data[offset + 3], data[offset + 2], data[offset + 1], data[offset + 0] };
            return BitConverter.ToSingle(tempBuffer, 0);
        }

        public static void SetFloat(this byte[] data, float value, int offset = 0)
        {
            var intValue = BitConverter.SingleToInt32Bits(value);
            SetInt32(data, intValue, offset);
        }
    }
}
