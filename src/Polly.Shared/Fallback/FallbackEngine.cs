using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            Func<Context, CancellationToken, TResult> fallbackAction)
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
                if (!shouldHandleExceptionPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(ex);
            }

            onFallback(delegateOutcome, context);

            return fallbackAction(context, cancellationToken);
        }
    }
}
