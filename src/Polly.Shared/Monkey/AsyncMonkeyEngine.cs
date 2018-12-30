using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Monkey
{
    internal class AsyncMonkeyEngine
    {
        private static async Task<bool> ShouldInjectAsync(Context context, Func<Context, Task<double>> injectionRate, Func<Context, Task<bool>> enabled, bool continueOnCapturedContext)
        {
            if (!await enabled(context).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            double injectionThreshold = await injectionRate(context).ConfigureAwait(continueOnCapturedContext);
            if (injectionThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey policies should always be a double between [0, 1]; never a negative number.");
            }
            if (injectionThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey policies should always be a double between [0, 1]; never a number greater than 1.");
            }

            return  ThreadSafeRandom_LockOncePerThread.NextDouble() < injectionThreshold;
        }

        internal static async Task<TResult> InjectBehaviourImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task> injectedBehaviour,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await ShouldInjectAsync(context, injectionRate, enabled, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext))
            {
                await injectedBehaviour(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> InjectExceptionImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task<Exception>> injectedException,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await ShouldInjectAsync(context, injectionRate, enabled, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext))
            {
                Exception exception = await injectedException(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> InjectResultImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task<TResult>> injectedResult,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await ShouldInjectAsync(context, injectionRate, enabled, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext))
            {
                return await injectedResult(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
