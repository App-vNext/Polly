#nullable enable
namespace Polly.Caching;

internal static class CacheEngine
{
    [DebuggerDisableUserUnhandledExceptions]
    internal static TResult Implementation<TResult>(
        ISyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Func<Context, CancellationToken, TResult> action,
        Context context,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError,
        CancellationToken cancellationToken)
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
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
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
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                onCachePutError?.Invoke(context, cacheKey, ex);
            }
        }

        return result;
    }
}
