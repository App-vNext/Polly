using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker
{
    internal class CircuitBreakerPolicy
    {
        internal static void Implementation(Action action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, ICircuitBreakerState breakerState)
        {
            if (breakerState.IsBroken)
            {
                throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerState.LastException);
            }

            try
            {
                action();
                breakerState.Reset();
            }
            catch (Exception ex)
            {
                if (!shouldRetryPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                breakerState.TryBreak(ex);

                throw;
            }
        }

#if NET45
        internal static async Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, ICircuitBreakerState breakerState)
        {
            if (breakerState.IsBroken)
            {
                throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerState.LastException);
            }

            try
            {
                await action();
                breakerState.Reset();
            }
            catch (Exception ex)
            {
                if (!shouldRetryPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                breakerState.TryBreak(ex);

                throw;
            }
        }
#else
        internal static Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, ICircuitBreakerState breakerState)
        {
            throw new NotSupportedException();
        }
#endif
    }
}