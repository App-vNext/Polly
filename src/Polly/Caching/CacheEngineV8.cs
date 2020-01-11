using System;
using System.Threading;

namespace Polly.Caching
{
    internal static class CacheEngineV8
    {
        internal static TResult Implementation<TExecutable, TResult>(
            ISyncCacheProvider<TResult> cacheProvider,
            ITtlStrategy<TResult> ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
            where TExecutable : ISyncExecutable<TResult>
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy(context);
            if (cacheKey == null)
            {
                return action.Execute(context, cancellationToken);
            }

            bool cacheHit;
            TResult valueFromCache;
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
                onCacheGet?.Invoke(context, cacheKey);
                return valueFromCache;
            }
            else
            {
                onCacheMiss?.Invoke(context, cacheKey);
            }

            TResult result = action.Execute(context, cancellationToken);

            Ttl ttl = ttlStrategy.GetTtl(context, result);
            if (ttl.Timespan > TimeSpan.Zero)
            {
                try
                {
                    cacheProvider.Put(cacheKey, result, ttl);
                    onCachePut?.Invoke(context, cacheKey);
                }
                catch (Exception ex)
                {
                    onCachePutError?.Invoke(context, cacheKey, ex);
                }
            }

            return result;
        }
    }
}