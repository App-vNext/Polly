#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action, 
            Context context,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates,
            ICircuitController<TResult> breakerController,
            CancellationToken cancellationToken, 
            bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            breakerController.OnActionPreExecute();

            try
            {
                DelegateResult<TResult> delegateOutcome = new DelegateResult<TResult>(await action(cancellationToken).ConfigureAwait(continueOnCapturedContext));

                cancellationToken.ThrowIfCancellationRequested();

                if (shouldHandleResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                {
                    breakerController.OnActionFailure(delegateOutcome, context);
                }
                else
                {
                    breakerController.OnActionSuccess(context);
                }

                return delegateOutcome.Result;
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

                breakerController.OnActionFailure(new DelegateResult<TResult>(ex), context);

                throw;
            }

        }

    }
}

#endif