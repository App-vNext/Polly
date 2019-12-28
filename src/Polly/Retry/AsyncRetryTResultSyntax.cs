using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Retry;

namespace Polly
{
    /// <summary>
    ///     Fluent API for defining an <see cref="AsyncRetryPolicy{TResult}" />.
    /// </summary>
    public static class AsyncRetryTResultSyntax
    {
        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
            => policyBuilder.RetryAsync(1);

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount)
        {
            return policyBuilder.RetryAsync(retryCount, onRetry: (Action<DelegateResult<TResult>, int>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry once
        ///     calling <paramref name="onRetry" /> on retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry)
            => policyBuilder.RetryAsync(1,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : async(outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry once
        ///     calling <paramref name="onRetryAsync" /> on retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
            => policyBuilder.RetryAsync(1, onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : (outcome, i, ctx) => onRetryAsync(outcome, i));

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int> onRetry)
        {
            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : async (outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(retryCount, onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : (outcome, i, ctx) => onRetryAsync(outcome, i));
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once
        /// calling <paramref name="onRetry"/> on retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry)
            => policyBuilder.RetryAsync(1, onRetry);

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once
        /// calling <paramref name="onRetryAsync"/> on retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
            => policyBuilder.RetryAsync(1, onRetryAsync);

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int, Context> onRetry)
        {
            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : async(outcome, i, ctx) => onRetry(outcome, i, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        public static IAsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, i, ctx),
                retryCount
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely until the action succeeds.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
        {
            return policyBuilder.RetryForeverAsync(onRetry: (Action<DelegateResult<TResult>>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, Context, Task>)null : async(DelegateResult<TResult> outcome, Context ctx) => onRetry(outcome)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : async(outcome, i, context) => onRetry(outcome, i)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Task> onRetryAsync)
        {
            return policyBuilder.RetryForeverAsync(onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, Context, Task>)null : (DelegateResult<TResult> outcome, Context ctx) => onRetryAsync(outcome));
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryForeverAsync(onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : (outcome, i, context) => onRetryAsync(outcome, i));
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, Context> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, Context, Task>)null : async(outcome, ctx) => onRetry(outcome, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, Context, Task>)null : async(outcome, i, ctx) => onRetry(outcome, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, Task> onRetryAsync)
        {
            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, ctx)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
        {
            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, i, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
        {
            return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetry: (Action<DelegateResult<TResult>, TimeSpan>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, span, i, ctx) => onRetry(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, span, i, ctx) => onRetryAsync(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationsEnumerable: sleepDurations
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc), result of previous execution, and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationProvider: sleepDurationProvider
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations)
        {
            return policyBuilder.WaitAndRetryAsync(sleepDurations, onRetry: (Action<DelegateResult<TResult>, TimeSpan>)null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, timespan, i, ctx) => onRetry(outcome, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : async(outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException(nameof(sleepDurations));

            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync,
                sleepDurationsEnumerable: sleepDurations
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, onRetry: (Action<DelegateResult<TResult>, TimeSpan>) null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, onRetry: (Action<DelegateResult<TResult>, TimeSpan, Context>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetry: onRetry == null ? (Action<DelegateResult<TResult>, TimeSpan, Context>)null : (outcome, timespan, context) => onRetry(outcome, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetry: onRetry == null ? (Action<DelegateResult<TResult>, int, TimeSpan, Context>)null : (outcome, i, timespan, context) => onRetry(outcome, i, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, Context, Task>)null : (outcome, timespan, context) => onRetryAsync(outcome, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and retry count.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, int, TimeSpan, Context, Task>)null : (outcome, i, timespan, context) => onRetryAsync(outcome, i, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, TimeSpan, Context, Task>)null : async (outcome, timespan, ctx) => onRetry(outcome, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<DelegateResult<TResult>, int, TimeSpan, Context, Task>)null : async(outcome, i, timespan, ctx) => onRetry(outcome, i, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            
            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx),
                sleepDurationProvider: sleepDurationProvider);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds, 
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return new AsyncRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (exception, timespan, i, ctx) => onRetryAsync(exception, i, timespan, ctx),
                sleepDurationProvider: sleepDurationProvider
            );
        }
    }
}

