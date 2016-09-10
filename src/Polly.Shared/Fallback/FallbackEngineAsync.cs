using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Fallback
{
    internal static partial class FallbackEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            Context context,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates,
            Func<DelegateResult<TResult>, Context, Task> onFallbackAsync,
            Func<CancellationToken, Task<TResult>> fallbackAction,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DelegateResult<TResult> delegateOutcome;

            try
            {
                delegateOutcome = new DelegateResult<TResult>(await action(cancellationToken).ConfigureAwait(continueOnCapturedContext));

                cancellationToken.ThrowIfCancellationRequested();

                if (!shouldHandleResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                {
                    return delegateOutcome.Result;
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    if (ex is OperationCanceledException && ((OperationCanceledException) ex).CancellationToken == cancellationToken)
                    {
                        throw;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (!shouldHandleExceptionPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(ex);
            }

            await onFallbackAsync(delegateOutcome, context).ConfigureAwait(continueOnCapturedContext);

            cancellationToken.ThrowIfCancellationRequested();

            return await fallbackAction(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
