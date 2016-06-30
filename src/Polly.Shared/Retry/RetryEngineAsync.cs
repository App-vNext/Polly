#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal static partial class RetryEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action, 
            CancellationToken cancellationToken, 
            IEnumerable<ExceptionPredicate> shouldRetryExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldRetryResultPredicates,
            Func<IRetryPolicyState<TResult>> policyStateFactory, 
            bool continueOnCapturedContext)
        {
            IRetryPolicyState<TResult> policyState = policyStateFactory();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    DelegateResult<TResult> delegateOutcome = new DelegateResult<TResult>(await action(cancellationToken).ConfigureAwait(continueOnCapturedContext));

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!shouldRetryResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                    {
                        return delegateOutcome.Result;
                    }

                    if (!await policyState
                        .CanRetryAsync(delegateOutcome, cancellationToken, continueOnCapturedContext)
                        .ConfigureAwait(continueOnCapturedContext))
                    {
                        return delegateOutcome.Result;
                    }
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (ex is OperationCanceledException && ((OperationCanceledException)ex).CancellationToken == cancellationToken)
                        {
                            throw;
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (!shouldRetryExceptionPredicates.Any(predicate => predicate(ex)))
                    {
                        throw;
                    }

                    if (!await policyState
                        .CanRetryAsync(new DelegateResult<TResult>(ex), cancellationToken, continueOnCapturedContext)
                        .ConfigureAwait(continueOnCapturedContext))
                    {
                        throw;
                    }
                }
            }
        }
    }
}

#endif
