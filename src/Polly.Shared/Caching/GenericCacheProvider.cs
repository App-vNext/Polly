using System;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a strongly-typed wrapper over a non-generic CacheProvider.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    public class GenericCacheProvider<TCacheFormat> : ICacheProvider<TCacheFormat>
    {
        private readonly ICacheProvider _wrappedCacheProvider;

        internal GenericCacheProvider(ICacheProvider nonGenericCacheProvider)
        {
            if (nonGenericCacheProvider == null) throw new ArgumentNullException(nameof(nonGenericCacheProvider));

            _wrappedCacheProvider = nonGenericCacheProvider;
        }

        TCacheFormat ICacheProvider<TCacheFormat>.Get(string key)
        {
            return (TCacheFormat) _wrappedCacheProvider.Get(key);
        }

        void ICacheProvider<TCacheFormat>.Put(string key, TCacheFormat value, Ttl ttl)
        {
            _wrappedCacheProvider.Put(key, value, ttl);
        }
    }
}
