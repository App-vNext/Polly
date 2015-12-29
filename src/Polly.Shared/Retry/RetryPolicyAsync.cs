#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal static partial class RetryPolicy
    {
        public static async Task ImplementationAsync(Func<Task> action, IEnumerable<ExceptionPredicate> shouldRetryPredicates, Func<IRetryPolicyState> policyStateFactory, bool continueOnCapturedContext)
        {
            IRetryPolicyState policyState = policyStateFactory();

            while (true)
            {
                try
                {
                    await action().ConfigureAwait(continueOnCapturedContext);
                    return;
                }
                catch (Exception ex)
                {
                    if (!shouldRetryPredicates.Any(predicate => predicate(ex)))
                    {
                        throw;
                    }

                    if (!(await policyState
                        .CanRetryAsync(ex, continueOnCapturedContext)
                        .ConfigureAwait(continueOnCapturedContext)))
                    {
                        throw;
                    }
                }
            }
        }
    }
}

#endif