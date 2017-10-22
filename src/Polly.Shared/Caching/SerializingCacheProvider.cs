using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines an <see cref="ISyncCacheProvider"/> which serializes objects of any type in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with synchronous <see cref="CachePolicy" />.
    /// </summary>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    public class SerializingCacheProvider<TSerialized> : ISyncCacheProvider
    {
        private readonly ISyncCacheProvider<TSerialized> _wrappedCacheProvider;
        private readonly ICacheItemSerializer<object, TSerialized> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializingCacheProvider{TResult, TSerialized}" /> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">wrappedCacheProvider </exception>
        /// <exception cref="System.ArgumentNullException">serializer </exception>
        public SerializingCacheProvider(ISyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _wrappedCacheProvider = wrappedCacheProvider;
            _serializer = serializer;
        }

        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The value from cache; or null, if none was found.</returns>
        public object Get(string key)
        {
            return _serializer.Deserialize(_wrappedCacheProvider.Get(key));
        }

        /// <summary>
        /// Puts the specified value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        public void Put(string key, object value, Ttl ttl)
        {
            _wrappedCacheProvider.Put(key, _serializer.Serialize(value), ttl);
        }

    }

    /// <summary>
    /// Defines an <see cref="ISyncCacheProvider{TResult}"/> which serializes objects of type <typeparamref name="TResult"/> in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with synchronous <see cref="CachePolicy" />.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    public class SerializingCacheProvider<TResult, TSerialized> : ISyncCacheProvider<TResult>
    {
        private readonly ISyncCacheProvider<TSerialized> _wrappedCacheProvider;
        private readonly ICacheItemSerializer<TResult, TSerialized> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializingCacheProvider{TResult, TSerialized}" /> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">wrappedCacheProvider </exception>
        /// <exception cref="System.ArgumentNullException">serializer </exception>
        public SerializingCacheProvider(ISyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _wrappedCacheProvider = wrappedCacheProvider;
            _serializer = serializer;
        }

        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The value from cache; or null, if none was found.</returns>
        public TResult Get(string key)
        {
            return _serializer.Deserialize(_wrappedCacheProvider.Get(key));
        }

        /// <summary>
        /// Puts the specified value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        public void Put(string key, TResult value, Ttl ttl)
        {
            _wrappedCacheProvider.Put(key, _serializer.Serialize(value), ttl);
        }
        
    }
}
