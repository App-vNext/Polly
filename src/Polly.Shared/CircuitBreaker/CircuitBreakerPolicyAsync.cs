#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Extensions;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerPolicy
    {
        internal static async Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, ICircuitBreakerState breakerState)
        {
            if (breakerState.IsBroken)
            {
                throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerState.LastException);
            }

            try
            {
                await action().NotOnCapturedContext();

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
    }
}

#endif