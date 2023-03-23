#if !NETCOREAPP

using System;

namespace Polly.Core.Tests.Utils;

/// <summary>
/// Utility that serializes and deserializes the exceptions using the binary formatter.
/// </summary>
internal class BinarySerializationUtil
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
