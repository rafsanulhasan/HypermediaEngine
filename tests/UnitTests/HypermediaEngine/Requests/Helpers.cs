using System.Buffers;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Requests;

internal static class Helpers
{
    extension(decimal value)
    {
        public ReadOnlySequence<byte> WriteDecimal()
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream))
            {
                writer.WriteNumberValue(value);
            }
            return new ReadOnlySequence<byte>(stream.ToArray());
        }
    }
}
