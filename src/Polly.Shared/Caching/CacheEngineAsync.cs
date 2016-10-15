using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    internal static partial class CacheEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            ICacheProviderAsync cacheProvider,
            ICacheKeyStrategy cacheKeyStrategy,
            Func<CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy.GetCacheKey(context);
            if (cacheKey == null)
            {
                return await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            object valueFromCache = await cacheProvider.GetAsync(cacheKey, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            if (valueFromCache != null) 
            {
                return (TResult)valueFromCache;
            }

            TResult result = await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            await cacheProvider.PutAsync(cacheKey, result, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            return result;
        }
    }
}