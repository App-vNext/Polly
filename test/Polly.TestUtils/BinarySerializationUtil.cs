#if NETFRAMEWORK

using System;

namespace Polly.TestUtils;

#pragma warning disable CA2300 // Do not use insecure deserializer BinaryFormatter
#pragma warning disable CA2301 // Do not use insecure deserializer BinaryFormatter

/// <summary>
/// Utility that serializes and deserializes the exceptions using the binary formatter.
/// </summary>
public static class BinarySerializationUtil
{
    public static T SerializeAndDeserializeException<T>(T exception)
        where T : Exception
    {
        using var stream = new MemoryStream();
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        formatter.Serialize(stream, exception);
        stream.Position = 0;
        return (T)formatter.Deserialize(stream);
    }
}
#endif
