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
    }
}
