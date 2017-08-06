using System;
using System.Threading;

namespace Polly.Caching
{
    internal static partial class CacheEngine
    {
        internal static TResult Implementation<TResult>(
            ISyncCacheProvider<TResult> cacheProvider,
            ITtlStrategy ttlStrategy,
            ICacheKeyStrategy cacheKeyStrategy,
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy.GetCacheKey(context);
            if (cacheKey == null)
            {
                return action(context, cancellationToken);
            }

            TResult valueFromCache = cacheProvider.Get(cacheKey);
            if (valueFromCache != null)
            {
                return valueFromCache;
            }

            TResult result = action(context, cancellationToken);

            Ttl ttl = ttlStrategy.GetTtl(context);
            if (ttl.Timespan > TimeSpan.Zero)
            {
                cacheProvider.Put(cacheKey, result, ttl);
            }

            return result;
        }
    }
}