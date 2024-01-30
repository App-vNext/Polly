#nullable enable
namespace Polly.Caching;

internal static class CacheEngine
{
    internal static TResult Implementation<TResult>(
        ISyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string cacheKey = cacheKeyStrategy(context);
        if (cacheKey == null)
        {
            return action(context, cancellationToken);
        }

        bool cacheHit;
        TResult? valueFromCache;
        try
        {
            (cacheHit, valueFromCache) = cacheProvider.TryGet(cacheKey);
        }
        catch (Exception ex)
        {
            cacheHit = false;
            valueFromCache = default;
            onCacheGetError?.Invoke(context, cacheKey, ex);
        }

        if (cacheHit)
        {
            onCacheGet(context, cacheKey);
            return valueFromCache!;
        }
        else
        {
            onCacheMiss(context, cacheKey);
        }

        TResult result = action(context, cancellationToken);

        Ttl ttl = ttlStrategy.GetTtl(context, result);
        if (ttl.Timespan > TimeSpan.Zero)
        {
            try
            {
                cacheProvider.Put(cacheKey, result, ttl);
                onCachePut(context, cacheKey);
            }
            catch (Exception ex)
            {
                onCachePutError?.Invoke(context, cacheKey, ex);
            }
        }

        return result;
    }
}
