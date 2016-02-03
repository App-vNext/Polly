using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static void Implementation(Action action, Context context, IEnumerable<ExceptionPredicate> shouldHandlePredicates, ICircuitBreakerState breakerState)
        {
            ThrowIfCircuitBroken(breakerState);

            try
            {
                action();
                breakerState.OnActionSuccess(context);
            }
            catch (Exception ex)
            {
                if (!shouldHandlePredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                breakerState.OnActionFailure(ex, context);

                throw;
            }
        }

        private static void ThrowIfCircuitBroken(ICircuitBreakerState breakerState)
        {
            switch (breakerState.CircuitState)
            {
                case CircuitState.Closed:
                case CircuitState.HalfOpen:
                    break;
                case CircuitState.Open:
                    throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerState.LastException);
                case CircuitState.Isolated:
                    throw new IsolatedCircuitException("The circuit is manually held open and is not allowing calls.");
                default:
                    throw new InvalidOperationException("Unhandled CircuitState.");
            }
        }
    }
}