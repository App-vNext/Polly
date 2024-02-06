#nullable enable
namespace Polly.Retry;

internal static class RetryEngine
{
    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken,
        ExceptionPredicates shouldRetryExceptionPredicates,
        ResultPredicates<TResult> shouldRetryResultPredicates,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry,
        int permittedRetryCount = int.MaxValue,
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
        Func<int, DelegateResult<TResult>, Context, TimeSpan>? sleepDurationProvider = null)
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
                    TResult result = action(context, cancellationToken);

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

                onRetry(outcome, waitDuration, tryCount, context);

                if (waitDuration > TimeSpan.Zero)
                {
                    SystemClock.Sleep(waitDuration, cancellationToken);
                }
            }

        }
        finally
        {
            sleepDurationsEnumerator?.Dispose();
        }
    }
}
