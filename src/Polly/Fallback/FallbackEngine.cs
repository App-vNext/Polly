using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

namespace Polly.Fallback
{
    internal static partial class FallbackEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates,
            Action<DelegateResult<TResult>, Context> onFallback,
            Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction)
        {
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                TResult result = action(context, cancellationToken);

                if (!shouldHandleResultPredicates.Any(predicate => predicate(result)))
                {
                    return result;
                }

                delegateOutcome = new DelegateResult<TResult>(result);
            }
            catch (Exception ex)
            {
                Exception handledException = shouldHandleExceptionPredicates
                    .Select(predicate => predicate(ex))
                    .FirstOrDefault(e => e != null);
                if (handledException == null)
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(handledException);
            }

            onFallback(delegateOutcome, context);

            return fallbackAction(delegateOutcome, context, cancellationToken);
        }
    }
}
