using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.Retry
{
    internal static partial class RetryEngine
    {
        internal static TResult Implementation<TResult>(
            Func<TResult> action,
            IEnumerable<ExceptionPredicate> shouldRetryExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldRetryResultPredicates,
            Func<IRetryPolicyState<TResult>> policyStateFactory)
        {
            IRetryPolicyState<TResult> policyState = policyStateFactory();

            while (true)
            {
                try
                {
                    DelegateResult<TResult> delegateOutcome = new DelegateResult<TResult>(action());

                    if (!shouldRetryResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                    {
                        return delegateOutcome.Result;
                    }

                    if (!policyState.CanRetry(delegateOutcome))
                    {
                        return delegateOutcome.Result;
                    }
                }
                catch (Exception ex)
                {
                    if (!shouldRetryExceptionPredicates.Any(predicate => predicate(ex)))
                    {
                        throw;
                    }

                    if (!policyState.CanRetry(new DelegateResult<TResult>(ex)))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
