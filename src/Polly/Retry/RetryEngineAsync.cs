using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

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
                    Exception handledException = shouldRetryExceptionPredicates
                        .Select(predicate => predicate(ex))
                        .FirstOrDefault(e => e != null);
                    if (handledException == null)
                    {
                        throw;
                    }

                    if (!await policyState
                        .CanRetryAsync(new DelegateResult<TResult>(handledException), cancellationToken, continueOnCapturedContext)
                        .ConfigureAwait(continueOnCapturedContext))
                    {
                        if (handledException != ex)
                        {
                            ExceptionDispatchInfo.Capture(handledException).Throw();
                        }
                        throw;
                    }
                }
            }
        }
    }
}


