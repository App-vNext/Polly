#nullable enable
namespace Polly.Caching;

internal static class AsyncCacheEngine
{
    internal static async Task<TResult> ImplementationAsync<TResult>(
        IAsyncCacheProvider<TResult> cacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        bool continueOnCapturedContext,
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
            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        bool cacheHit;
        TResult? valueFromCache;
        try
        {
            (cacheHit, valueFromCache) = await cacheProvider.TryGetAsync(cacheKey, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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

        TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

        Ttl ttl = ttlStrategy.GetTtl(context, result);
        if (ttl.Timespan > TimeSpan.Zero)
        {
            try
            {
                await cacheProvider.PutAsync(cacheKey, result, ttl, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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
