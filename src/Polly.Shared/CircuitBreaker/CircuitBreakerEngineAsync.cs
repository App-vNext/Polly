

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
            Func<Context, CancellationToken, Task<TResult>> action, 
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
                TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                if (shouldHandleResultPredicates.Any(predicate => predicate(result)))
                {
                    breakerController.OnActionFailure(new DelegateResult<TResult>(result), context);
                }
                else
                {
                    breakerController.OnActionSuccess(context);
                }

                return result;
            }
            catch (Exception ex)
            {
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

