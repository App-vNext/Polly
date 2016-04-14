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
        internal static async Task ImplementationAsync(Func<CancellationToken, Task> action, Context context, IEnumerable<ExceptionPredicate> shouldHandlePredicates, ICircuitController breakerController, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();

            breakerController.OnActionPreExecute();

            try
            {
                await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);

                breakerController.OnActionSuccess(context);
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

                breakerController.OnActionFailure(ex, context);

                throw;
            }
        }

    }
}

#endif