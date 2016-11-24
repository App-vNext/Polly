using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines methods for classes providing mapping of cached values for synchronous cache functionality for Polly <see cref="CachePolicy"/>s.
    /// </summary>
    public abstract class MappingCacheProvider : ICacheProvider
    {
        private readonly ICacheProvider _wrappedCacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingCacheProvider"/> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        protected MappingCacheProvider(ICacheProvider wrappedCacheProvider)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));

            _wrappedCacheProvider = wrappedCacheProvider;
        }

        /// <summary>
        /// Maps an object retrieved from the cache back to the native or original format before it was cached. 
        /// </summary>
        /// <param name="rawObjectFromCache">The raw object from cache.</param>
        /// <returns>The object mapped back to native format.</returns>
        public abstract object GetMapper(object rawObjectFromCache);

        /// <summary>
        /// Maps a native object to an amended format for caching.
        /// </summary>
        /// <param name="nativeObject">The native object.</param>
        /// <returns>The object mapped to caching format.</returns>
        public abstract object PutMapper(object nativeObject);

        object ICacheProvider.Get(string key)
        {
            return GetMapper(_wrappedCacheProvider.Get(key));
        }

        void ICacheProvider.Put(string key, object value, TimeSpan ttl)
        {
            _wrappedCacheProvider.Put(key, PutMapper(value), ttl);
        }
    }
    
    /// <summary>
    /// Defines methods for classes providing mapping of cached values for synchronous cache functionality for Polly <see cref="CachePolicy"/>s.
    /// </summary>
    public abstract class MappingCacheProvider<TNative, TMapped> : ICacheProvider<TNative>
    {
        private readonly ICacheProvider<TMapped> _wrappedCacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingCacheProvider{TNative, TMapped}"/> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        protected MappingCacheProvider(ICacheProvider<TMapped> wrappedCacheProvider)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));

            _wrappedCacheProvider = wrappedCacheProvider;
        }

        /// <summary>
        /// Maps an object retrieved from the cache back to the native or original format before it was cached. 
        /// </summary>
        /// <param name="rawObjectFromCache">The raw object from cache.</param>
        /// <returns>The object mapped back to native format.</returns>
        public abstract TNative GetMapper(TMapped rawObjectFromCache);

        /// <summary>
        /// Maps a native object to an amended format for caching.
        /// </summary>
        /// <param name="nativeObject">The native object.</param>
        /// <returns>The object mapped to caching format.</returns>
        public abstract TMapped PutMapper(TNative nativeObject);

        TNative ICacheProvider<TNative>.Get(string key)
        {
            return GetMapper(_wrappedCacheProvider.Get(key));
        }

        void ICacheProvider<TNative>.Put(string key, TNative value, TimeSpan ttl)
        {
            _wrappedCacheProvider.Put(key, PutMapper(value), ttl);
        }
    }
}
