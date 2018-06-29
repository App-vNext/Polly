using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// Provides a strongly-typed wrapper over a non-generic CacheProviderAsync.
    /// </summary>
    /// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
    internal class GenericCacheProviderAsync<TCacheFormat> : IAsyncCacheProvider<TCacheFormat>
    {
        private readonly IAsyncCacheProvider _wrappedCacheProvider;

        internal GenericCacheProviderAsync(IAsyncCacheProvider nonGenericCacheProvider)
        {
            _wrappedCacheProvider = nonGenericCacheProvider ?? throw new ArgumentNullException(nameof(nonGenericCacheProvider));
        }

        async Task<TCacheFormat> IAsyncCacheProvider<TCacheFormat>.GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return (TCacheFormat) (await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext) ?? default(TCacheFormat));
        }

        Task IAsyncCacheProvider<TCacheFormat>.PutAsync(string key, TCacheFormat value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return _wrappedCacheProvider.PutAsync(key, value, ttl, cancellationToken, continueOnCapturedContext);
        }
    }
}
