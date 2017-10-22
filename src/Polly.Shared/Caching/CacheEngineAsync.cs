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
            Func<Context, string> cacheKeyStrategy,
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
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
                return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            TResult valueFromCache;
            try
            {
                valueFromCache = await cacheProvider.GetAsync(cacheKey, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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

            TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            Ttl ttl = ttlStrategy.GetTtl(context);
            if (ttl.Timespan > TimeSpan.Zero)
            {
                try
                {
                    await cacheProvider.PutAsync(cacheKey, result, ttl, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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