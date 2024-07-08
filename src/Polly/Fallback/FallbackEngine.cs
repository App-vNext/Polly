#nullable enable

namespace Polly.Fallback;

internal static class FallbackEngine
{
    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> action,
        Context context,
        ExceptionPredicates shouldHandleExceptionPredicates,
        ResultPredicates<TResult> shouldHandleResultPredicates,
        Action<DelegateResult<TResult>, Context> onFallback,
        Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction,
        CancellationToken cancellationToken)
    {
        DelegateResult<TResult> delegateOutcome;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            TResult result = action(context, cancellationToken);

            if (!shouldHandleResultPredicates.AnyMatch(result))
            {
                return result;
            }

            delegateOutcome = new DelegateResult<TResult>(result);
        }
        catch (Exception ex)
        {
            Exception handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
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
