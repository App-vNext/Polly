using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a strongly-typed wrapper over a non-generic CacheProviderAsync.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    public class GenericCacheProviderAsync<TCacheFormat> : ICacheProviderAsync<TCacheFormat>
    {
        private readonly ICacheProviderAsync _wrappedCacheProvider;

        internal GenericCacheProviderAsync(ICacheProviderAsync nonGenericCacheProvider)
        {
            if (nonGenericCacheProvider == null) throw new ArgumentNullException(nameof(nonGenericCacheProvider));

            _wrappedCacheProvider = nonGenericCacheProvider;
        }

        async Task<TCacheFormat> ICacheProviderAsync<TCacheFormat>.GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return (TCacheFormat) await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }

        Task ICacheProviderAsync<TCacheFormat>.PutAsync(string key, TCacheFormat value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return _wrappedCacheProvider.PutAsync(key, value, ttl, cancellationToken, continueOnCapturedContext);
        }
    }
}
