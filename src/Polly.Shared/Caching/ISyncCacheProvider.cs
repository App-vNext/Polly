using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines methods for classes providing synchronous cache functionality for Polly <see cref="CachePolicy"/>s.
    /// </summary>
    public interface ISyncCacheProvider
    {
        /// <summary>
        /// Gets a value from cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The value from cache; or null, if none was found.</returns>
        object Get(String key);

        /// <summary>
        /// Puts the specified value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        void Put(string key, object value, Ttl ttl);
    }

    /// <summary>
    /// Defines methods for classes providing synchronous cache functionality for Polly <see cref="CachePolicy{TResult}"/>s.
    /// </summary>
    public interface ISyncCacheProvider<TResult>
    {
        /// <summary>
        /// Gets a value from cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The value from cache; or null, if none was found.</returns>
        TResult Get(String key);

        /// <summary>
        /// Puts the specified value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        void Put(string key, TResult value, Ttl ttl);
    }
}