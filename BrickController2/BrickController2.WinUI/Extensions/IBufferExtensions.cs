using Windows.Storage.Streams;

namespace BrickController2.Windows.Extensions;

public static class IBufferExtensions
{
    public static IBuffer ToBuffer(this byte[] data)
    {
        var writer = new DataWriter();
        writer.WriteBytes(data);

        return writer.DetachBuffer();
    }

    public static byte[] ToByteArray(this IBuffer buffer)
    {
        using (var reader = DataReader.FromBuffer(buffer))
        {
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);

            return input;
        }
    }
}
