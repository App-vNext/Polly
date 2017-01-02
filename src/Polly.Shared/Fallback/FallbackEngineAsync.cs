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
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                delegateOutcome = new DelegateResult<TResult>(await action(cancellationToken).ConfigureAwait(continueOnCapturedContext));

                if (!shouldHandleResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                {
                    return delegateOutcome.Result;
                }
            }
            catch (Exception ex)
            {
                if (!shouldHandleExceptionPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(ex);
            }

            await onFallbackAsync(delegateOutcome, context).ConfigureAwait(continueOnCapturedContext);

            return await fallbackAction(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
