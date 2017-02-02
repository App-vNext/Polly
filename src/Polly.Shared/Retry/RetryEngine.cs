using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Polly.Retry
{
    internal static partial class RetryEngine
    {
        internal static TResult Implementation<TResult>(
            Func<CancellationToken, TResult> action,
            CancellationToken cancellationToken,
            IEnumerable<ExceptionPredicate> shouldRetryExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldRetryResultPredicates,
            Func<IRetryPolicyState<TResult>> policyStateFactory)
        {
            IRetryPolicyState<TResult> policyState = policyStateFactory();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    TResult result = action(cancellationToken);

                    if (!shouldRetryResultPredicates.Any(predicate => predicate(result)))
                    {
                        return result;
                    }

                    if (!policyState.CanRetry(new DelegateResult<TResult>(result), cancellationToken))
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

                    if (!policyState.CanRetry(new DelegateResult<TResult>(ex), cancellationToken))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
