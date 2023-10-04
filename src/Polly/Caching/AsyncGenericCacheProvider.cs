#nullable enable
namespace Polly.Caching;

/// <summary>
/// Provides a strongly-typed wrapper over a non-generic CacheProviderAsync.
/// </summary>
/// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
internal class AsyncGenericCacheProvider<TCacheFormat> : IAsyncCacheProvider<TCacheFormat>
{
    private readonly IAsyncCacheProvider _wrappedCacheProvider;

    internal AsyncGenericCacheProvider(IAsyncCacheProvider nonGenericCacheProvider) =>
        _wrappedCacheProvider = nonGenericCacheProvider ?? throw new ArgumentNullException(nameof(nonGenericCacheProvider));

    async Task<(bool, TCacheFormat?)> IAsyncCacheProvider<TCacheFormat>.TryGetAsync(string key, bool continueOnCapturedContext, CancellationToken cancellationToken)
    {
        (bool cacheHit, object? result) = await _wrappedCacheProvider.TryGetAsync(key, continueOnCapturedContext, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        return (cacheHit, (TCacheFormat?)(result ?? default(TCacheFormat)));
    }

    Task IAsyncCacheProvider<TCacheFormat>.PutAsync(string key, TCacheFormat? value, Ttl ttl, bool continueOnCapturedContext, CancellationToken cancellationToken) =>
        _wrappedCacheProvider.PutAsync(key, value, ttl, continueOnCapturedContext, cancellationToken);
}
