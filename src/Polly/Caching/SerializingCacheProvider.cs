#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines an <see cref="ISyncCacheProvider"/> which serializes objects of any type in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with synchronous <see cref="CachePolicy" />.
/// </summary>
/// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
public class SerializingCacheProvider<TSerialized> : ISyncCacheProvider
{
    private readonly ISyncCacheProvider<TSerialized> _wrappedCacheProvider;
    private readonly ICacheItemSerializer<object, TSerialized> _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializingCacheProvider{TSerialized}"/> class.
    /// </summary>
    /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
    /// <param name="serializer">The serializer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wrappedCacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializer"/> is <see langword="null"/>.</exception>
    public SerializingCacheProvider(ISyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
    {
        _wrappedCacheProvider = wrappedCacheProvider ?? throw new ArgumentNullException(nameof(wrappedCacheProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>
    /// A tuple whose first element is a value indicating whether the key was found in the cache,
    /// and whose second element is the value from the cache (null if not found).
    /// </returns>
    public (bool, object?) TryGet(string key)
    {
        (bool cacheHit, TSerialized? objectToDeserialize) = _wrappedCacheProvider.TryGet(key);
        return (cacheHit, cacheHit ? _serializer.Deserialize(objectToDeserialize) : null);
    }

    /// <summary>
    /// Puts the specified value in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    public void Put(string key, object? value, Ttl ttl) =>
        _wrappedCacheProvider.Put(key, _serializer.Serialize(value), ttl);
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wrappedCacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializer"/> is <see langword="null"/>.</exception>
    public SerializingCacheProvider(ISyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
    {
        _wrappedCacheProvider = wrappedCacheProvider ?? throw new ArgumentNullException(nameof(wrappedCacheProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>
    /// A tuple whose first element is a value indicating whether the key was found in the cache,
    /// and whose second element is the value from the cache (default(TResult) if not found).
    /// </returns>
    public (bool, TResult?) TryGet(string key)
    {
        (bool cacheHit, TSerialized? objectToDeserialize) = _wrappedCacheProvider.TryGet(key);
        return (cacheHit, cacheHit ? _serializer.Deserialize(objectToDeserialize) : default);
    }

    /// <summary>
    /// Puts the specified value in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    public void Put(string key, TResult? value, Ttl ttl) =>
        _wrappedCacheProvider.Put(key, _serializer.Serialize(value), ttl);
}
