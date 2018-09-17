using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

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
                Exception handledException = shouldHandleExceptionPredicates
                    .Select(predicate => predicate(ex))
                    .FirstOrDefault(e => e != null);
                if (handledException == null)
                {
                    throw;
                }

                breakerController.OnActionFailure(new DelegateResult<TResult>(handledException), context);

                if (handledException != ex)
                {
                    ExceptionDispatchInfo.Capture(handledException).Throw();
                }
                throw;
            }

        }

    }
}

