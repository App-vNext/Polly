using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static TResult Implementation<TResult>(
            Func<CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates, 
            ICircuitController<TResult> breakerController)
        {
            breakerController.OnActionPreExecute();

            try
            {
                DelegateResult<TResult> delegateOutcome = new DelegateResult<TResult>(action(cancellationToken));

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