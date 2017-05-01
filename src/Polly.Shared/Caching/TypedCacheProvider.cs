using System;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a typed wrapper over a non-generic CacheProvider.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    public class TypedCacheProvider<TCacheFormat> : ICacheProvider<TCacheFormat>
    {
        private readonly ICacheProvider _wrappedCacheProvider;

        internal TypedCacheProvider(ICacheProvider nonGenericCacheProvider)
        {
            if (nonGenericCacheProvider == null) throw new ArgumentNullException(nameof(nonGenericCacheProvider));

            _wrappedCacheProvider = nonGenericCacheProvider;
        }

        TCacheFormat ICacheProvider<TCacheFormat>.Get(string key)
        {
            return (TCacheFormat) _wrappedCacheProvider.Get(key);
        }

        void ICacheProvider<TCacheFormat>.Put(string key, TCacheFormat value, TimeSpan ttl)
        {
            _wrappedCacheProvider.Put(key, value, ttl);
        }
    }
}
