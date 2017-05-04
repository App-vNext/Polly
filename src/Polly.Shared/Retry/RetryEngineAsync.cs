

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
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context, 
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
                    TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                    if (!shouldRetryResultPredicates.Any(predicate => predicate(result)))
                    {
                        return result;
                    }

                    if (!await policyState
                        .CanRetryAsync(new DelegateResult<TResult>(result), cancellationToken, continueOnCapturedContext)
                        .ConfigureAwait(continueOnCapturedContext))
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
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


