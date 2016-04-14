using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static void Implementation(Action action, Context context, IEnumerable<ExceptionPredicate> shouldHandlePredicates, ICircuitController breakerController)
        {
            breakerController.OnActionPreExecute();

            try
            {
                action();
                breakerController.OnActionSuccess(context);
            }
            catch (Exception ex)
            {
                if (!shouldHandlePredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                breakerController.OnActionFailure(ex, context);

                throw;
            }
        }
    }
}