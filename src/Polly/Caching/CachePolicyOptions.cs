using System;

namespace Polly.Caching
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="ICachePolicy"/>.
    /// </summary>
    public sealed class CachePolicyOptions
    {
        /// <summary>
        /// The cache key strategy.
        /// </summary>
        public Func<Context, string> CacheKeyStrategy { get; set; }

        /// <summary>
        /// Delegate to call on a cache hit, when value is returned from cache.
        /// </summary>
        public Action<Context, string> OnCacheGet { get; set; }

        /// <summary>
        /// Delegate to call on a cache miss.
        /// </summary>
        public Action<Context, string> OnCacheMiss { get; set; }

        /// <summary>
        /// Delegate to call on cache put.
        /// </summary>
        public Action<Context, string> OnCachePut { get; set; }

        /// <summary>
        /// Delegate to call if an exception is thrown when attempting to get a value from the cache, passing the execution context, the cache key, and the exception.
        /// </summary>
        public Action<Context, string, Exception> OnCacheGetError { get; set; }

        /// <summary>
        /// Delegate to call if an exception is thrown when attempting to put a value in the cache, passing the execution context, the cache key, and the exception.
        /// </summary>
        public Action<Context, string, Exception> OnCachePutError { get; set; }
    }
}
