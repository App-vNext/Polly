using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a typed wrapper over a non-generic CacheProviderAsync.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    public class TypedCacheProviderAsync<TCacheFormat> : ICacheProviderAsync<TCacheFormat>
    {
        private readonly ICacheProviderAsync _wrappedCacheProvider;

        internal TypedCacheProviderAsync(ICacheProviderAsync nonGenericCacheProvider)
        {
            if (nonGenericCacheProvider == null) throw new ArgumentNullException(nameof(nonGenericCacheProvider));

            _wrappedCacheProvider = nonGenericCacheProvider;
        }

        async Task<TCacheFormat> ICacheProviderAsync<TCacheFormat>.GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return (TCacheFormat) await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }

        Task ICacheProviderAsync<TCacheFormat>.PutAsync(string key, TCacheFormat value, TimeSpan ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return _wrappedCacheProvider.PutAsync(key, value, ttl, cancellationToken, continueOnCapturedContext);
        }
    }
}
