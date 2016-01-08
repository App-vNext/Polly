#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerPolicy
    {
        internal static async Task ImplementationAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, IEnumerable<ExceptionPredicate> shouldHandlePredicates, ICircuitBreakerState breakerState, bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (breakerState.IsBroken)
            {
                throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", breakerState.LastException);
            }

            try
            {
                await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);

                breakerState.Reset();
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

                if (!shouldHandlePredicates.Any(predicate => predicate(ex)))
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