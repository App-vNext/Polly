#nullable enable
namespace Polly;

public partial class Policy
{
    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="Context.OperationKey"/>.
    /// If the <paramref name="cacheProvider"/> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider cacheProvider, TimeSpan ttl, Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="Context.OperationKey"/>.
    /// If the <paramref name="cacheProvider"/> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider cacheProvider, ITtlStrategy ttlStrategy, Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        TimeSpan ttl,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), cacheKeyStrategy.GetCacheKey, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        ITtlStrategy ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, cacheKeyStrategy.GetCacheKey, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider cacheProvider, TimeSpan ttl, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), cacheKeyStrategy, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider cacheProvider, ITtlStrategy ttlStrategy, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, cacheKeyStrategy, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        TimeSpan ttl,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        ITtlStrategy ttlStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        TimeSpan ttl,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), cacheKeyStrategy.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        ITtlStrategy ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, cacheKeyStrategy.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        TimeSpan ttl,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), new RelativeTtl(ttl), cacheKeyStrategy, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider cacheProvider,
        ITtlStrategy ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        return CacheAsync<TResult>(cacheProvider.AsyncFor<TResult>(), ttlStrategy, cacheKeyStrategy, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="Context.OperationKey"/>.
    /// If the <paramref name="cacheProvider"/> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, TimeSpan ttl, Action<Context, string, Exception>? onCacheError = null) =>
        CacheAsync<TResult>(cacheProvider, new RelativeTtl(ttl), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="Context.OperationKey"/>.
    /// If the <paramref name="cacheProvider"/> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, ITtlStrategy ttlStrategy, Action<Context, string, Exception>? onCacheError = null) =>
        CacheAsync<TResult>(cacheProvider, ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="Context.OperationKey"/>.
    /// If the <paramref name="cacheProvider"/> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, ITtlStrategy<TResult> ttlStrategy, Action<Context, string, Exception>? onCacheError = null) =>
        CacheAsync<TResult>(cacheProvider, ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        TimeSpan ttl,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(cacheProvider, new RelativeTtl(ttl), cacheKeyStrategy.GetCacheKey, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        Action<Context, string> emptyDelegate = (_, _) => { };

        return CacheAsync<TResult>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy.GetCacheKey,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            onCacheError,
            onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string, Exception>? onCacheError = null)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        Action<Context, string> emptyDelegate = (_, _) => { };

        return CacheAsync<TResult>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy.GetCacheKey,
            emptyDelegate,
            emptyDelegate,
            emptyDelegate,
            onCacheError,
            onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, TimeSpan ttl, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception>? onCacheError = null) =>
        CacheAsync<TResult>(cacheProvider, new RelativeTtl(ttl), cacheKeyStrategy, onCacheError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, ITtlStrategy ttlStrategy, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception>? onCacheError = null)
    {
        Action<Context, string> emptyDelegate = (_, _) => { };

        return CacheAsync<TResult>(cacheProvider, ttlStrategy, cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, onCacheError, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(IAsyncCacheProvider<TResult> cacheProvider, ITtlStrategy<TResult> ttlStrategy, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception>? onCacheError = null)
    {
        Action<Context, string> emptyDelegate = (_, _) => { };

        return CacheAsync<TResult>(cacheProvider, ttlStrategy, cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, onCacheError, onCacheError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        TimeSpan ttl,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError) =>
        CacheAsync<TResult>(cacheProvider, new RelativeTtl(ttl), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheGet, onCacheMiss,
            onCachePut, onCacheGetError, onCachePutError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy ttlStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError) =>
        CacheAsync<TResult>(cacheProvider, ttlStrategy.For<TResult>(), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError) =>
        CacheAsync<TResult>(cacheProvider, ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        TimeSpan ttl,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(
            cacheProvider,
            new RelativeTtl(ttl),
            cacheKeyStrategy.GetCacheKey,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(
            cacheProvider,
            ttlStrategy.For<TResult>(),
            cacheKeyStrategy.GetCacheKey,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        ICacheKeyStrategy cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheKeyStrategy is null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        return CacheAsync<TResult>(
            cacheProvider,
            ttlStrategy,
            cacheKeyStrategy.GetCacheKey,
            onCacheGet,
            onCacheMiss,
            onCachePut,
            onCacheGetError,
            onCachePutError);
    }

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttl">Duration (ttl) for which to cache values.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        TimeSpan ttl,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError) =>
        CacheAsync<TResult>(cacheProvider, new RelativeTtl(ttl), cacheKeyStrategy, onCacheGet, onCacheMiss,
            onCachePut, onCacheGetError, onCachePutError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError) =>
        CacheAsync<TResult>(cacheProvider, ttlStrategy.For<TResult>(), cacheKeyStrategy, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);

    /// <summary>
    /// <para>Builds an <see cref="AsyncPolicy{TResult}" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
    /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
    /// If the <paramref name="cacheProvider" /> provides a value from cache, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cacheProvider">The cache provider.</param>
    /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
    /// <param name="cacheKeyStrategy">The cache key strategy.</param>
    /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
    /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
    /// <param name="onCachePut">Delegate to call on cache put.</param>
    /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
    /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ttlStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKeyStrategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheGet"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCacheMiss"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onCachePut"/> is <see langword="null"/>.</exception>
    public static AsyncCachePolicy<TResult> CacheAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        if (cacheProvider == null)
        {
            throw new ArgumentNullException(nameof(cacheProvider));
        }

        if (ttlStrategy == null)
        {
            throw new ArgumentNullException(nameof(ttlStrategy));
        }

        if (cacheKeyStrategy == null)
        {
            throw new ArgumentNullException(nameof(cacheKeyStrategy));
        }

        if (onCacheGet == null)
        {
            throw new ArgumentNullException(nameof(onCacheGet));
        }

        if (onCacheMiss == null)
        {
            throw new ArgumentNullException(nameof(onCacheMiss));
        }

        if (onCachePut == null)
        {
            throw new ArgumentNullException(nameof(onCachePut));
        }

        return new AsyncCachePolicy<TResult>(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
    }
}
