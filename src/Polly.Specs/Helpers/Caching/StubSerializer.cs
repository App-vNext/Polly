using System;
using Polly.Caching;

namespace Polly.Specs.Helpers.Caching
{
    /// <summary>
    ///  A configurable stub serializer implementation to support tests around serializing cache providers.
    /// </summary>
    /// <typeparam name="TResult">The type of the results being cached.</typeparam>
    /// <typeparam name="TSerialized">The type of the serialized values.</typeparam>
    internal class StubSerializer<TResult, TSerialized> : ICacheItemSerializer<TResult, TSerialized>
    {
        private readonly Func<TResult, TSerialized> _serialize;
        private readonly Func<TSerialized, TResult> _deserialize;

        public StubSerializer(Func<TResult, TSerialized> serialize, Func<TSerialized, TResult> deserialize)
        {
            _serialize = serialize;
            _deserialize = deserialize;
        }
        public TSerialized Serialize(TResult objectToSerialize) => _serialize(objectToSerialize);

        public TResult Deserialize(TSerialized objectToDeserialize) => _deserialize(objectToDeserialize);
    }
}
