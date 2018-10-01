using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    internal static partial class MonkeyEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context,
            CancellationToken cancellationToken,
            Func<Context, Task> fault,
            Func<Context, Task<Decimal>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context) && GetRandomNumber() < await injectionRate(context))
            {
                await fault(context);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, Task<Exception>> fault,
            Func<Context, Task<Decimal>> injectionRate,
            Func<Context, Task<bool>> enabled,
            bool continueOnCapturedContext)
        {
            if (await enabled(context) && GetRandomNumber() < await injectionRate(context))
            {
                throw await fault(context);
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
