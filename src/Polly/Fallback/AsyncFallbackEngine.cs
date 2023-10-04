#nullable enable
namespace Polly.Fallback;

internal class AsyncFallbackEngine
{
    internal static async Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        ExceptionPredicates shouldHandleExceptionPredicates,
        ResultPredicates<TResult> shouldHandleResultPredicates,
        Func<DelegateResult<TResult>, Context, Task> onFallbackAsync,
        Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        DelegateResult<TResult> delegateOutcome;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

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

        await onFallbackAsync(delegateOutcome, context).ConfigureAwait(continueOnCapturedContext);

        return await fallbackAction(delegateOutcome, context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
    }
}
