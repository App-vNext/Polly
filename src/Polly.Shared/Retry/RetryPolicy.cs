using System;
using System.Collections.Generic;
using System.Linq;

namespace Polly.Retry
{
    internal static partial class RetryPolicy
    {
        public static void Implementation(Action action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, Func<IRetryPolicyState> policyStateFactory)
        {
            var policyState = policyStateFactory();

            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    if (!shouldRetryPredicates.Any(predicate => predicate(ex)))
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