using Polly.Caching;
using System;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>.
        /// If the <paramref name="cacheProvider"/> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        public static CachePolicy CacheAsync(IAsyncCacheProvider cacheProvider, TimeSpan ttl, Action<Context, string, Exception> onCacheError = null)
        {
            return CacheAsync(cacheProvider, new RelativeTtl(ttl), DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>
        /// If the <paramref name="cacheProvider"/> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        public static CachePolicy CacheAsync(IAsyncCacheProvider cacheProvider, ITtlStrategy ttlStrategy, Action<Context, string, Exception> onCacheError = null)
        {
            return CacheAsync(cacheProvider, ttlStrategy, DefaultCacheKeyStrategy.Instance.GetCacheKey, onCacheError);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider"/> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy CacheAsync(IAsyncCacheProvider cacheProvider, TimeSpan ttl, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception> onCacheError = null)
        {
            return CacheAsync(cacheProvider, new RelativeTtl(ttl), cacheKeyStrategy, onCacheError);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <param name="onCacheError">Delegate to call if an exception is thrown when attempting to get a value from or put a value into the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy CacheAsync(IAsyncCacheProvider cacheProvider, ITtlStrategy ttlStrategy, Func<Context, string> cacheKeyStrategy, Action<Context, string, Exception> onCacheError = null)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            if (ttlStrategy == null) throw new ArgumentNullException(nameof(ttlStrategy));
            if (cacheKeyStrategy == null) throw new ArgumentNullException(nameof(cacheKeyStrategy));

            onCacheError = onCacheError ?? ((_, __, ___) => { });
            Action<Context, string> emptyDelegate = (_, __) => { };

            return new CachePolicy(cacheProvider, ttlStrategy, cacheKeyStrategy, emptyDelegate, emptyDelegate, emptyDelegate, onCacheError, onCacheError);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
        /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
        /// <param name="onCachePut">Delegate to call on cache put.</param>
        /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
        /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        /// <exception cref="ArgumentNullException">onCacheGet</exception>
        /// <exception cref="ArgumentNullException">onCacheMiss</exception>
        /// <exception cref="ArgumentNullException">onCachePut</exception>
        /// <exception cref="ArgumentNullException">onCacheGetError</exception>
        /// <exception cref="ArgumentNullException">onCachePutError</exception>
        public static CachePolicy CacheAsync(
            IAsyncCacheProvider cacheProvider, 
            ITtlStrategy ttlStrategy, 
            ICacheKeyStrategy cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
        {
            return CacheAsync(cacheProvider, ttlStrategy, cacheKeyStrategy.GetCacheKey, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a result.</para>
        /// <para>Before executing a delegate returning a result, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> provides a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <param name="onCacheGet">Delegate to call on a cache hit, when value is returned from cache.</param>
        /// <param name="onCacheMiss">Delegate to call on a cache miss.</param>
        /// <param name="onCachePut">Delegate to call on cache put.</param>
        /// <param name="onCacheGetError">Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.</param>
        /// <param name="onCachePutError">Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        /// <exception cref="ArgumentNullException">onCacheGet</exception>
        /// <exception cref="ArgumentNullException">onCacheMiss</exception>
        /// <exception cref="ArgumentNullException">onCachePut</exception>
        /// <exception cref="ArgumentNullException">onCacheGetError</exception>
        /// <exception cref="ArgumentNullException">onCachePutError</exception>
        public static CachePolicy CacheAsync(
            IAsyncCacheProvider cacheProvider,
            ITtlStrategy ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            if (ttlStrategy == null) throw new ArgumentNullException(nameof(ttlStrategy));
            if (cacheKeyStrategy == null) throw new ArgumentNullException(nameof(cacheKeyStrategy));

            if (onCacheGet == null) throw new ArgumentNullException(nameof(onCacheGet));
            if (onCacheMiss == null) throw new ArgumentNullException(nameof(onCacheMiss));
            if (onCachePut == null) throw new ArgumentNullException(nameof(onCachePut));
            if (onCachePutError == null) throw new ArgumentNullException(nameof(onCachePutError));
            if (onCachePutError == null) throw new ArgumentNullException(nameof(onCachePutError));

            return new CachePolicy(cacheProvider, ttlStrategy, cacheKeyStrategy, onCacheGet, onCacheMiss, onCachePut, onCacheGetError, onCachePutError);
        }
    }
}
