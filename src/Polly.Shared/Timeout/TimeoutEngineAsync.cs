using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Timeout
{
    internal static partial class TimeoutEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context, 
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync, 
            CancellationToken cancellationToken, 
            bool continueOnCapturedContext)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TimeSpan timeout = timeoutProvider(context);

            using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                using (CancellationTokenSource combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
                {
                    Task<TResult> actionTask = null;
                    CancellationToken combinedToken = combinedTokenSource.Token;

                    try
                    {
                        if (timeoutStrategy == TimeoutStrategy.Optimistic)
                        {
                            SystemClock.CancelTokenAfter(timeoutCancellationTokenSource, timeout);
                            return await action(context, combinedToken).ConfigureAwait(continueOnCapturedContext);
                        }

                        // else: timeoutStrategy == TimeoutStrategy.Pessimistic

                        Task<TResult> timeoutTask = timeoutCancellationTokenSource.Token.AsTask<TResult>();

                        SystemClock.CancelTokenAfter(timeoutCancellationTokenSource, timeout);

                        actionTask = action(context, combinedToken);

                        return await (await 
#if NET40
                            TaskEx
#else
                            Task
#endif
                            .WhenAny(actionTask, timeoutTask).ConfigureAwait(continueOnCapturedContext)).ConfigureAwait(continueOnCapturedContext);

                    }
                    catch (Exception e)
                    {
                        if (timeoutCancellationTokenSource.IsCancellationRequested)
                        {
                            await onTimeoutAsync(context, timeout, actionTask).ConfigureAwait(continueOnCapturedContext);
                            throw new TimeoutRejectedException("The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.", e);
                        }

                        throw;
                    }
                }
            }
        }

        private static Task<TResult> AsTask<TResult>(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<TResult>();

            // A generalised version of this method would include a hotpath returning a canceled task (rather than setting up a registration) if (cancellationToken.IsCancellationRequested) on entry.  This is omitted, since we only start the timeout countdown in the token _after calling this method.

            IDisposable registration = null;
                registration = cancellationToken.Register(() =>
                {
                    tcs.TrySetCanceled();
                    registration?.Dispose();
                }, useSynchronizationContext: false);

            return tcs.Task;
        }
    }
}
