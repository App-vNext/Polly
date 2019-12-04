﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class AsyncCircuitBreakerEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext,
            ExceptionPredicates shouldHandleExceptionPredicates, 
            ResultPredicates<TResult> shouldHandleResultPredicates,
            ICircuitController<TResult> breakerController)
        {
            cancellationToken.ThrowIfCancellationRequested();

            breakerController.OnActionPreExecute();

            try
            {
                TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

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

                handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                throw;
            }
        }
    }
}

