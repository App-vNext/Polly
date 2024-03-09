#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines operations for serializing and deserializing values being placed in caches by <see cref="CachePolicy" /> instances.
/// </summary>
/// <typeparam name="TResult">The type of objects that this serializer can serialize.</typeparam>
/// <typeparam name="TSerialized">The type of objects after serialization.</typeparam>
public interface ICacheItemSerializer<TResult, TSerialized>
{
    /// <summary>
    /// Serializes the specified object.
    /// </summary>
    /// <param name="objectToSerialize">The object to serialize.</param>
    /// <returns>The serialized object.</returns>
    TSerialized? Serialize(TResult? objectToSerialize);

    /// <summary>
    /// Deserializes the specified object.
    /// </summary>
    /// <param name="objectToDeserialize">The object to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    TResult? Deserialize(TSerialized? objectToDeserialize);
}
