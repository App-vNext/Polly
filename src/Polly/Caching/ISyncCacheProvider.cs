﻿using System;

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
        /// <returns>
        /// A tuple whose first element is a value indicating whether the key was found in the cache,
        /// and whose second element is the value from the cache (null if not found).
        /// </returns>
        (bool, object) TryGet(String key);

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
        /// <returns>
        /// A tuple whose first element is a value indicating whether the key was found in the cache,
        /// and whose second element is the value from the cache (default(TResult) if not found).
        /// </returns>
        (bool, TResult) TryGet(String key);

        /// <summary>
        /// Puts the specified value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        void Put(string key, TResult value, Ttl ttl);
    }
}