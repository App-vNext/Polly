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
            if (await enabled(context) && RandomGenerator.GetRandomNumber() < await injectionRate(context))
            {
                await fault(context, cancellationToken);
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
            if (await enabled(context) && RandomGenerator.GetRandomNumber() < await injectionRate(context))
            {
                throw await fault(context, cancellationToken);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, CancellationToken, Task<DelegateResult<TResult>>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context) && RandomGenerator.GetRandomNumber() < await injectionRate(context))
            {
                DelegateResult<TResult> faultResponse = await fault(context, cancellationToken);
                if (faultResponse.Exception != null)
                {
                    throw faultResponse.Exception;
                }

                return faultResponse.Result;
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
