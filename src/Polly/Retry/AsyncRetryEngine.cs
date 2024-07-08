#nullable enable
namespace Polly.Retry;

internal static class AsyncRetryEngine
{
    internal static async Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        ExceptionPredicates shouldRetryExceptionPredicates,
        ResultPredicates<TResult> shouldRetryResultPredicates,
        Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync,
        CancellationToken cancellationToken,
        int permittedRetryCount = int.MaxValue,
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
        Func<int, DelegateResult<TResult>, Context, TimeSpan>? sleepDurationProvider = null,
        bool continueOnCapturedContext = false)
    {
        int tryCount = 0;
        IEnumerator<TimeSpan>? sleepDurationsEnumerator = sleepDurationsEnumerable?.GetEnumerator();

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool canRetry;
                DelegateResult<TResult> outcome;

                try
                {
                    TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                    if (!shouldRetryResultPredicates.AnyMatch(result))
                    {
                        return result;
                    }

                    canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerator == null || sleepDurationsEnumerator.MoveNext());

                    if (!canRetry)
                    {
                        return result;
                    }

                    outcome = new DelegateResult<TResult>(result);
                }
                catch (Exception ex)
                {
                    Exception handledException = shouldRetryExceptionPredicates.FirstMatchOrDefault(ex);
                    if (handledException == null)
                    {
                        throw;
                    }

                    canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerator == null || sleepDurationsEnumerator.MoveNext());

                    if (!canRetry)
                    {
                        handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                        throw;
                    }

                    outcome = new DelegateResult<TResult>(handledException);
                }

                if (tryCount < int.MaxValue)
                {
                    tryCount++;
                }

                TimeSpan waitDuration = sleepDurationsEnumerator?.Current ?? (sleepDurationProvider?.Invoke(tryCount, outcome, context) ?? TimeSpan.Zero);

                await onRetryAsync(outcome, waitDuration, tryCount, context).ConfigureAwait(continueOnCapturedContext);

                if (waitDuration > TimeSpan.Zero)
                {
                    await SystemClock.SleepAsync(waitDuration, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                }
            }
        }
        finally
        {
            sleepDurationsEnumerator?.Dispose();
        }
    }
}
