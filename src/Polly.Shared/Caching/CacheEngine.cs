using System;
using System.Threading;

namespace Polly.Caching
{
    internal static partial class CacheEngine
    {
        internal static TResult Implementation<TResult>(
            ICacheProvider<TResult> cacheProvider,
            ICacheKeyStrategy cacheKeyStrategy,
            Func<CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy.GetCacheKey(context);
            if (cacheKey == null)
            {
                return action(cancellationToken);
            }

            object valueFromCache = cacheProvider.Get(cacheKey);
            if (valueFromCache != null)
            {
                return (TResult) valueFromCache;
            }

            TResult result = action(cancellationToken);
            cacheProvider.Put(cacheKey, result);
            return result;
        }
    }
}