using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

#if NET40
using ExceptionDispatchInfo = Polly.Utilities.ExceptionDispatchInfo;
#endif

namespace Polly.Timeout
{
    internal static partial class TimeoutEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Action<Context, TimeSpan, Task> onTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TimeSpan timeout = timeoutProvider(context);

            using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                using (CancellationTokenSource combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
                {
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

                        actionTask = 

                        #if NET40
                            TaskEx
                        #else
                            Task
                        #endif                             

                        .Run(() =>
                            action(context, combinedToken)       // cancellation token here allows the user delegate to react to cancellation: possibly clear up; then throw an OperationCanceledException.
                            , combinedToken);           // cancellation token here only allows Task.Run() to not begin the passed delegate at all, if cancellation occurs prior to invoking the delegate.  
                        try
                        {
                            actionTask.Wait(timeoutCancellationTokenSource.Token); // cancellation token here cancels the Wait() and causes it to throw, but does not cancel actionTask.  We use only timeoutCancellationTokenSource.Token here, not combinedToken.  If we allowed the user's cancellation token to cancel the Wait(), in this pessimistic scenario where the user delegate may not observe that cancellation, that would create a no-longer-observed task.  That task could in turn later fault before completing, risking an UnobservedTaskException.
                        }
                        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1) // Issue #270.  Unwrap extra AggregateException caused by the way pessimistic timeout policy for synchronous executions is necessarily constructed.
                        {
                            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        }

                        return actionTask.Result;
                    }
                    catch (Exception ex)
                    {
                        if (timeoutCancellationTokenSource.IsCancellationRequested)
                        {
                            onTimeout(context, timeout, actionTask);
                            throw new TimeoutRejectedException("The delegate executed through TimeoutPolicy did not complete within the timeout.", ex);
                        }

                        throw;
                    }
                }
            }
        }

    }
}