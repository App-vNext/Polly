using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Monkey
{
    internal static partial class MonkeyEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context).ConfigureAwait(continueOnCapturedContext) && ThreadSafeRandom_LockOncePerThread.NextDouble() < await injectionRate(context).ConfigureAwait(continueOnCapturedContext))
            {
                await fault(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task<Exception>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context).ConfigureAwait(continueOnCapturedContext) && ThreadSafeRandom_LockOncePerThread.NextDouble() < await injectionRate(context).ConfigureAwait(continueOnCapturedContext))
            {
                throw await fault(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task<TResult>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context).ConfigureAwait(continueOnCapturedContext) && ThreadSafeRandom_LockOncePerThread.NextDouble() < await injectionRate(context).ConfigureAwait(continueOnCapturedContext))
            {
                return await fault(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
