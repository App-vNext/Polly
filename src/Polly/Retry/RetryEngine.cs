#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Polly.Retry.Options;
using Polly.Utilities;

namespace Polly.Retry
{
    internal static class RetryEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            ExceptionPredicates shouldRetryExceptionPredicates,
            ResultPredicates<TResult> shouldRetryResultPredicates,
            RetryInvocationHandlerBase<TResult>? retryInvocationHandler,
            RetryCountValue permittedRetryCount,
            SleepDurationProviderBase<TResult>? sleepDurationProvider = null)
        {
            int tryCount = 0;

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

                        canRetry = tryCount < permittedRetryCount;
                    
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

                        canRetry = tryCount < permittedRetryCount;

                        if (!canRetry)
                        {
                            handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                            throw;
                        }

                        outcome = new DelegateResult<TResult>(handledException);
                    }

                    if (tryCount < int.MaxValue) { tryCount++; }

                    TimeSpan waitDuration = sleepDurationProvider?.GetNext(tryCount, outcome, context) ?? TimeSpan.Zero;
                
                    retryInvocationHandler?.OnRetry(outcome, waitDuration, tryCount, context);

                    if (waitDuration > TimeSpan.Zero)
                    {
                        SystemClock.Sleep(waitDuration, cancellationToken);
                    }
                }

            }
            finally
            {
                sleepDurationProvider?.Dispose();
            }
        }
    }
}
