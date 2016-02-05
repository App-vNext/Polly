using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static void Implementation(Action action, Context context, IEnumerable<ExceptionPredicate> shouldHandlePredicates, ICircuitController breakerController)
        {
            ThrowIfCircuitBroken(breakerController);

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

        private static void ThrowIfCircuitBroken(ICircuitController breakerController)
        {
            switch (breakerController.CircuitState)
            {
                case CircuitState.Closed:
                case CircuitState.HalfOpen:
                    break;
                case CircuitState.Open:
                    throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerController.LastException);
                case CircuitState.Isolated:
                    throw new IsolatedCircuitException("The circuit is manually held open and is not allowing calls.");
                default:
                    throw new InvalidOperationException("Unhandled CircuitState.");
            }
        }
    }
}