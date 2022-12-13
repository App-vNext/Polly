using System;
using System.Threading;

namespace Polly.Fallback;

internal static class FallbackEngine
{
    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken,
        ExceptionPredicates shouldHandleExceptionPredicates,
        ResultPredicates<TResult> shouldHandleResultPredicates,
        Action<DelegateResult<TResult>, Context> onFallback,
        Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction)
    {
        DelegateResult<TResult> delegateOutcome;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = action(context, cancellationToken);

            if (!shouldHandleResultPredicates.AnyMatch(result))
            {
                return result;
            }

            delegateOutcome = new(result);
        }
        catch (Exception ex)
        {
            var handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
            if (handledException == null)
            {
                throw;
            }

            delegateOutcome = new(handledException);
        }

        onFallback(delegateOutcome, context);

        return fallbackAction(delegateOutcome, context, cancellationToken);
    }
}