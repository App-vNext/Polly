using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

namespace Polly.Retry
{
    internal static partial class RetryEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
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
                    TResult result = action(context, cancellationToken);

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
                    Exception handledException = shouldRetryExceptionPredicates
                        .Select(predicate => predicate(ex))
                        .FirstOrDefault(e => e != null);
                    if (handledException == null)
                    {
                        throw;
                    }

                    if (!policyState.CanRetry(new DelegateResult<TResult>(handledException), cancellationToken))
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
