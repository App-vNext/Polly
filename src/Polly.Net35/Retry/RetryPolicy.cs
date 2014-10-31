using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal static class RetryPolicy
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

#if NET45
        public static async Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, Func<IRetryPolicyState> policyStateFactory)
        {
            var policyState = policyStateFactory();

            while (true)
            {
                try
                {
                    await action();
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
#else
        internal static Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, Func<IRetryPolicyState> policyStateFactory)
        {
            throw new NotSupportedException();
        }
#endif
    }
}