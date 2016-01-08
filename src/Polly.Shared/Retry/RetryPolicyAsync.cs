#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal static partial class RetryPolicy
    {
        public static async Task ImplementationAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, IEnumerable<ExceptionPredicate> shouldRetryPredicates, Func<IRetryPolicyState> policyStateFactory, bool continueOnCapturedContext)
        {
            IRetryPolicyState policyState = policyStateFactory();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);

                    return;
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

                    if (!shouldRetryPredicates.Any(predicate => predicate(ex)))
                    {
                        throw;
                    }

                    if (!(await policyState
                        .CanRetryAsync(ex, cancellationToken, continueOnCapturedContext)
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