using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Timeout
{
    internal static partial class TimeoutEngine
    {
        internal static TResult Implementation<TResult>(
            Func<CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<TimeSpan> timeoutProvider,
            Action<Context, TimeSpan, Task> onTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TimeSpan timeout = timeoutProvider();

            using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                using (CancellationTokenSource combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
                {
                    CancellationToken combinedToken = combinedTokenSource.Token;

                    Task<TResult> actionTask = null;
                    try
                    {
                        timeoutCancellationTokenSource.CancelAfter(timeout);

                        //if (timeoutStrategy == TimeoutStrategy.Optimistic)
                        //{
                        //     return action(combinedToken);
                        //}
                        //else
                        //{

                        actionTask = 

                        #if NET40
                            TaskEx
                        #else
                            Task
                        #endif                             

                        .Run(() =>
                            action(combinedToken)       // cancellation token here allows the user delegate to react to cancellation: possibly clear up; then throw an OperationCanceledException.
                            , combinedToken);           // cancellation token here only allows Task.Run() to not begin the passed delegate at all, if cancellation occurs prior to invoking the delegate.  

                        actionTask.Wait(combinedToken); // cancellation token here cancels the Wait() and causes it to throw, but does not cancel actionTask.

                        //}

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