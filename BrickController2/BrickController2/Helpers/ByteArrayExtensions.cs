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
    }
}
