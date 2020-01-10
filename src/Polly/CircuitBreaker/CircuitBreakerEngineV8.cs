using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Polly.CircuitBreaker
{
    internal class CircuitBreakerEngineV8
    {
        internal static TResult Implementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken,
            ExceptionPredicates shouldHandleExceptionPredicates, 
            ResultPredicates<TResult> shouldHandleResultPredicates, 
            ICircuitController<TResult> breakerController)
            where TExecutable : ISyncExecutable<TResult>
        {
            cancellationToken.ThrowIfCancellationRequested();

            breakerController.OnActionPreExecute();

            try
            {
                TResult result = action.Execute(context, cancellationToken);

                if (shouldHandleResultPredicates.AnyMatch(result))
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
                Exception handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
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