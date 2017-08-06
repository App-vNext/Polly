using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    internal static partial class CacheEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            IAsyncCacheProvider<TResult> cacheProvider,
            ITtlStrategy ttlStrategy,
            ICacheKeyStrategy cacheKeyStrategy,
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy.GetCacheKey(context);
            if (cacheKey == null)
            {
                return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            TResult valueFromCache = await cacheProvider.GetAsync(cacheKey, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            if (valueFromCache != null) 
            {
                return valueFromCache;
            }

            TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            Ttl ttl = ttlStrategy.GetTtl(context);
            if (ttl.Timespan > TimeSpan.Zero)
            {
                await cacheProvider.PutAsync(cacheKey, result, ttl, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            }

            return result;
        }
    }
}