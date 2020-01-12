using System;
using System.Threading;

namespace Polly.Fallback
{
    internal static class FallbackEngine
    {
        internal static TResult Implementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken,
            ExceptionPredicates shouldHandleExceptionPredicates,
            ResultPredicates<TResult> shouldHandleResultPredicates,
            Action<DelegateResult<TResult>, Context> onFallback,
            Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction)
            where TExecutable : ISyncExecutable<TResult>
        {
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                TResult result = action.Execute(context, cancellationToken);

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

            onFallback?.Invoke(delegateOutcome, context);

            return fallbackAction(delegateOutcome, context, cancellationToken);
        }
    }
}
