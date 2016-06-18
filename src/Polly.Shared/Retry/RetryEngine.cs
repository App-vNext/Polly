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
            Func<IRetryPolicyState> policyStateFactory)
        {
            var policyState = policyStateFactory();

            while (true)
            {
                try
                {
                    TResult result = action();
                    if (!shouldRetryResultPredicates.Any(predicate => predicate(result)))
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

                    if (!policyState.CanRetry(ex))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
