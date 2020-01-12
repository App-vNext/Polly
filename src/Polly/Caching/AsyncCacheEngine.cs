using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    internal static class AsyncCacheEngine
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            IAsyncCacheProvider<TResult> cacheProvider,
            ITtlStrategy<TResult> ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy(context);
            if (cacheKey == null)
            {
                return await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            }

            bool cacheHit;
            TResult valueFromCache;
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
                onCacheGet?.Invoke(context, cacheKey);
                return valueFromCache;
            }
            else
            {
                onCacheMiss?.Invoke(context, cacheKey);
            }

            TResult result = await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

            Ttl ttl = ttlStrategy.GetTtl(context, result);
            if (ttl.Timespan > TimeSpan.Zero)
            {
                try
                {
                    await cacheProvider.PutAsync(cacheKey, result, ttl, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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