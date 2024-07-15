#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines an <see cref="IAsyncCacheProvider"/> which serializes objects of any type in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with asynchronous <see cref="CachePolicy" />.
/// </summary>
/// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
public class AsyncSerializingCacheProvider<TSerialized> : IAsyncCacheProvider
{
    private readonly IAsyncCacheProvider<TSerialized> _wrappedCacheProvider;
    private readonly ICacheItemSerializer<object, TSerialized> _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSerializingCacheProvider{TSerialized}"/> class.
    /// </summary>
    /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
    /// <param name="serializer">The serializer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wrappedCacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializer"/> is <see langword="null"/>.</exception>
    public AsyncSerializingCacheProvider(IAsyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
    {
        _wrappedCacheProvider = wrappedCacheProvider ?? throw new ArgumentNullException(nameof(wrappedCacheProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> promising as Result a tuple whose first element is a value indicating whether
    /// the key was found in the cache, and whose second element is the value from the cache (null if not found).
    /// </returns>
    public async Task<(bool, object?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        (bool cacheHit, TSerialized? objectToDeserialize) = await _wrappedCacheProvider.TryGetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        return (cacheHit, cacheHit ? _serializer.Deserialize(objectToDeserialize) : null);
    }

    /// <summary>
    /// Puts the specified value in the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
    /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
    public Task PutAsync(string key, object? value, Ttl ttl, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        _wrappedCacheProvider.PutAsync(
            key,
            _serializer.Serialize(value),
            ttl,
            cancellationToken,
            continueOnCapturedContext);
}

/// <summary>
/// Defines an <see cref="IAsyncCacheProvider{TResult}"/> which serializes objects of type <typeparamref name="TResult"/> in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with asynchronous <see cref="CachePolicy" />.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
/// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
public class AsyncSerializingCacheProvider<TResult, TSerialized> : IAsyncCacheProvider<TResult>
{
    private readonly IAsyncCacheProvider<TSerialized> _wrappedCacheProvider;
    private readonly ICacheItemSerializer<TResult, TSerialized> _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSerializingCacheProvider{TResult, TSerialized}"/> class.
    /// </summary>
    /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
    /// <param name="serializer">The serializer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wrappedCacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serializer"/> is <see langword="null"/>.</exception>
    public AsyncSerializingCacheProvider(IAsyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
    {
        _wrappedCacheProvider = wrappedCacheProvider ?? throw new ArgumentNullException(nameof(wrappedCacheProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> promising as Result a tuple whose first element is a value indicating whether
    /// the key was found in the cache, and whose second element is the value from the cache (default(TResult) if not found).
    /// </returns>
    public async Task<(bool, TResult?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        (bool cacheHit, TSerialized? objectToDeserialize) = await _wrappedCacheProvider.TryGetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        return (cacheHit, cacheHit ? _serializer.Deserialize(objectToDeserialize) : default);
    }

    /// <summary>
    /// Puts the specified value in the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
    /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
    public Task PutAsync(string key, TResult? value, Ttl ttl, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        _wrappedCacheProvider.PutAsync(
            key,
            _serializer.Serialize(value),
            ttl,
            cancellationToken,
            continueOnCapturedContext);
}
