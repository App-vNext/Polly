using System.Runtime.ExceptionServices;

namespace Polly.Timeout;

internal static class TimeoutEngine
{
    internal static TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action,
        Context context,
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TimeSpan timeout = timeoutProvider(context);

        using var timeoutCancellationTokenSource = new CancellationTokenSource();
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

        CancellationToken combinedToken = combinedTokenSource.Token;

        Task<TResult> actionTask = null;
        try
        {
            if (timeoutStrategy == TimeoutStrategy.Optimistic)
            {
                SystemClock.CancelTokenAfter(timeoutCancellationTokenSource, timeout);
                return action(context, combinedToken);
            }

            // else: timeoutStrategy == TimeoutStrategy.Pessimistic

            SystemClock.CancelTokenAfter(timeoutCancellationTokenSource, timeout);

            actionTask = Task.Run(() =>
                action(context, combinedToken),       // cancellation token here allows the user delegate to react to cancellation: possibly clear up; then throw an OperationCanceledException.
                combinedToken);           // cancellation token here only allows Task.Run() to not begin the passed delegate at all, if cancellation occurs prior to invoking the delegate.
            try
            {
                /*
                 * Cancellation token here cancels the Wait() and causes it to throw, but does not cancel actionTask.
                 * We use only timeoutCancellationTokenSource.Token here, not combinedToken.
                 * If we allowed the user's cancellation token to cancel the Wait(), in this pessimistic scenario where the user delegate may not observe that cancellation, that would create a no-longer-observed task.
                 * That task could in turn later fault before completing, risking an UnobservedTaskException.
                 */
                actionTask.Wait(timeoutCancellationTokenSource.Token);
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
            {
                // Issue #270. Unwrap extra AggregateException caused by the way pessimistic timeout policy for synchronous executions is necessarily constructed.
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }

            return actionTask.Result;
        }
        catch (Exception ex)
        {
            // Note that we cannot rely on testing (operationCanceledException.CancellationToken == combinedToken || operationCanceledException.CancellationToken == timeoutCancellationTokenSource.Token)
            // as either of those tokens could have been onward combined with another token by executed code, and so may not be the token expressed on operationCanceledException.CancellationToken.
            if (ex is OperationCanceledException && timeoutCancellationTokenSource.IsCancellationRequested)
            {
                onTimeout(context, timeout, actionTask, ex);
                throw new TimeoutRejectedException("The delegate executed through TimeoutPolicy did not complete within the timeout.", ex);
            }

            throw;
        }
    }
}
