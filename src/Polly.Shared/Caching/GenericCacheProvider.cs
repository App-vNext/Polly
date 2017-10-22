using System;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a strongly-typed wrapper over a non-generic CacheProvider.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    internal class GenericCacheProvider<TCacheFormat> : ISyncCacheProvider<TCacheFormat>
    {
        private readonly ISyncCacheProvider _wrappedCacheProvider;

        internal GenericCacheProvider(ISyncCacheProvider nonGenericCacheProvider)
        {
            if (nonGenericCacheProvider == null) throw new ArgumentNullException(nameof(nonGenericCacheProvider));

            _wrappedCacheProvider = nonGenericCacheProvider;
        }

        TCacheFormat ISyncCacheProvider<TCacheFormat>.Get(string key)
        {
            return (TCacheFormat) _wrappedCacheProvider.Get(key);
        }

        void ISyncCacheProvider<TCacheFormat>.Put(string key, TCacheFormat value, Ttl ttl)
        {
            _wrappedCacheProvider.Put(key, value, ttl);
        }
    }
}
