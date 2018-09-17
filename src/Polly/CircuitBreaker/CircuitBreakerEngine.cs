using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

namespace Polly.CircuitBreaker
{
    internal partial class CircuitBreakerEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates, 
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates, 
            ICircuitController<TResult> breakerController)
        {
            cancellationToken.ThrowIfCancellationRequested();

            breakerController.OnActionPreExecute();

            try
            {
                TResult result = action(context, cancellationToken);

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