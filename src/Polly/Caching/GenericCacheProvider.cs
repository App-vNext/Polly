#nullable enable
namespace Polly.Caching;

/// <summary>
/// Provides a strongly-typed wrapper over a non-generic CacheProvider.
/// </summary>
/// <typeparam name="TCacheFormat">The type of the objects in the cache.</typeparam>
internal sealed class GenericCacheProvider<TCacheFormat> : ISyncCacheProvider<TCacheFormat>
{
    private readonly ISyncCacheProvider _wrappedCacheProvider;

    internal GenericCacheProvider(ISyncCacheProvider nonGenericCacheProvider) =>
        _wrappedCacheProvider = nonGenericCacheProvider ?? throw new ArgumentNullException(nameof(nonGenericCacheProvider));

    (bool, TCacheFormat?) ISyncCacheProvider<TCacheFormat>.TryGet(string key)
    {
        (bool cacheHit, object? result) = _wrappedCacheProvider.TryGet(key);
        return (cacheHit, (TCacheFormat?)(result ?? default(TCacheFormat)));
    }

    void ISyncCacheProvider<TCacheFormat>.Put(string key, TCacheFormat? value, Ttl ttl) =>
        _wrappedCacheProvider.Put(key, value, ttl);
}
