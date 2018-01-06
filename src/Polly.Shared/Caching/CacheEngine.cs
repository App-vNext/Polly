using System;
using System.Threading;

namespace Polly.Caching
{
    internal static partial class CacheEngine
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
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy(context);
            if (cacheKey == null)
            {
                return action(context, cancellationToken);
            }

            TResult valueFromCache;
            try
            {
                valueFromCache = cacheProvider.Get(cacheKey);
            }
            catch (Exception ex)
            {
                valueFromCache = default(TResult);
                onCacheGetError(context, cacheKey, ex);
            }
            if (valueFromCache != null && !valueFromCache.Equals(default(TResult)))
            {
                onCacheGet(context, cacheKey);
                return valueFromCache;
            }
            else
            {
                onCacheMiss(context, cacheKey);
            }

            TResult result = action(context, cancellationToken);

            Ttl ttl = ttlStrategy.GetTtl(context, result);
            if (ttl.Timespan > TimeSpan.Zero && result != null && !result.Equals(default(TResult)))
            {
                try
                {
                    cacheProvider.Put(cacheKey, result, ttl);
                    onCachePut(context, cacheKey);
                }
                catch (Exception ex)
                {
                    onCachePutError(context, cacheKey, ex);
                }
            }

            return result;
        }
    }
}