using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Fallback
{
    internal class AsyncFallbackEngine
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            ExceptionPredicates shouldHandleExceptionPredicates,
            ResultPredicates<TResult> shouldHandleResultPredicates,
            Func<DelegateResult<TResult>, Context, Task> onFallbackAsync,
            Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                TResult result = await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

                if (!shouldHandleResultPredicates.AnyMatch(result))
                {
                    return result;
                }

                delegateOutcome = new DelegateResult<TResult>(result);
            }
            catch (Exception ex)
            {
                Exception handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
                if (handledException == null)
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(handledException);
            }

            if (onFallbackAsync != null) { await onFallbackAsync(delegateOutcome, context).ConfigureAwait(continueOnCapturedContext); }

            return await fallbackAction(delegateOutcome, context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
