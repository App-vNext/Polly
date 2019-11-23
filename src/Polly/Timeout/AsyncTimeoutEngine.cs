﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Timeout
{
    internal static class AsyncTimeoutEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context, 
            CancellationToken cancellationToken, 
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync, 
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

                        return await (await Task.WhenAny(actionTask, timeoutTask).ConfigureAwait(continueOnCapturedContext)).ConfigureAwait(continueOnCapturedContext);

                    }
                    catch (Exception ex)
                    {
                        // Note that we cannot rely on testing (operationCanceledException.CancellationToken == combinedToken || operationCanceledException.CancellationToken == timeoutCancellationTokenSource.Token)
                        // as either of those tokens could have been onward combined with another token by executed code, and so may not be the token expressed on operationCanceledException.CancellationToken.
                        if (ex is OperationCanceledException && timeoutCancellationTokenSource.IsCancellationRequested)
                        {
                            await onTimeoutAsync(context, timeout, actionTask, ex).ConfigureAwait(continueOnCapturedContext);
                            throw new TimeoutRejectedException("The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.", ex);
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
