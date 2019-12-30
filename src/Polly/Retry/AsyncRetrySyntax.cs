using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Retry;

namespace Polly
{
    /// <summary>
    ///     Fluent API for defining a <see cref="AsyncRetryPolicy" />.
    /// </summary>
    public static class AsyncRetrySyntax
    {
        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder)
            => policyBuilder.RetryAsync(1);

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount)
        {
            return policyBuilder.RetryAsync(retryCount, onRetry: (Action<Exception, int, Context>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry once
        ///     calling <paramref name="onRetry" /> on retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int> onRetry)
            => policyBuilder.RetryAsync(1,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, Context, Task>)null : async (outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry once
        ///     calling <paramref name="onRetryAsync" /> on retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync)
            => policyBuilder.RetryAsync(1, onRetryAsync: onRetryAsync == null ? (Func<Exception, int, Context, Task>)null : (outcome, i, ctx) => onRetryAsync(outcome, i));

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int> onRetry)
        {
            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, Context, Task>)null : async(outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(retryCount, onRetryAsync: onRetryAsync == null ? (Func<Exception, int, Context, Task>)null : (outcome, i, ctx) => onRetryAsync(outcome, i));
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once
        /// calling <paramref name="onRetry"/> on retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int, Context> onRetry)
            => policyBuilder.RetryAsync(1, onRetry);

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once
        /// calling <paramref name="onRetryAsync"/> on retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync)
            => policyBuilder.RetryAsync(1, onRetryAsync);

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int, Context> onRetry)
        {
            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, Context, Task>)null : async (outcome, i, ctx) => onRetry(outcome, i, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        public static IAsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, i, ctx),
                retryCount
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely until the action succeeds.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder)
        {
            return policyBuilder.RetryForeverAsync(onRetry: (Action<Exception>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, Context, Task>)null : async (Exception outcome, Context ctx) => onRetry(outcome)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, int> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, Context, Task>)null : async (outcome, i, context) => onRetry(outcome, i)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Task> onRetryAsync)
        {
            return policyBuilder.RetryForeverAsync(onRetryAsync: onRetryAsync == null ? (Func<Exception, Context, Task>)null : (Exception outcome, Context ctx) => onRetryAsync(outcome));
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryForeverAsync(onRetryAsync: onRetryAsync == null ? (Func<Exception, int, Context, Task>)null : (outcome, i, context) => onRetryAsync(outcome, i));
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, Context> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, Context, Task>)null : async (outcome, ctx) => onRetry(outcome, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, int, Context> onRetry)
        {
            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, Context, Task>)null : async (outcome, i, ctx) => onRetry(outcome, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Context, Task> onRetryAsync)
        {
            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, ctx)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync)
        {
            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, i, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry <paramref name="retryCount"/> times.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
        ///     the current retry number (1 for first retry, 2 for second etc).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
        {
            return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetry: (Action<Exception, TimeSpan>) null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, span, i, ctx) => onRetry(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, span, i, ctx) => onRetryAsync(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationsEnumerable: sleepDurations
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
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
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            
            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationProvider: sleepDurationProvider
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations)
        {
            return policyBuilder.WaitAndRetryAsync(sleepDurations, onRetry: (Action<Exception, TimeSpan, int, Context>)null);
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">
        ///     sleepDurations
        /// </exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">
        ///     sleepDurations
        /// </exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">
        ///     sleepDurations
        /// </exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, ctx)
#pragma warning restore 1998
            );

        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">
        ///     sleepDurations
        /// </exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurations</exception>
        public static IAsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException(nameof(sleepDurations));

            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync,
                sleepDurationsEnumerable: sleepDurations
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely until the action succeeds.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, onRetry: (Action<Exception, TimeSpan>) null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely until the action succeeds.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, onRetry: (Action<Exception, TimeSpan, Context>) null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetry: onRetry == null ? (Action<Exception, TimeSpan, Context>)null : (exception, timespan, context) => onRetry(exception, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
                onRetry: onRetry == null ? (Action<Exception, int, TimeSpan, Context>)null : (exception, i, timespan, context) => onRetry(exception, i, timespan)
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
               sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
               onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, Context, Task>)null : (exception, timespan, context) => onRetryAsync(exception, timespan)
         );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and retry count.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc)
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
             sleepDurationProvider: (retryCount, context) => sleepDurationProvider(retryCount),
             onRetryAsync: onRetryAsync == null ? (Func<Exception, int, TimeSpan, Context, Task>)null : (exception, i, timespan, context) => onRetryAsync(exception, i, timespan)
         );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, TimeSpan, Context, Task>)null : async (exception, timespan, ctx) => onRetry(exception, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan, Context> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: onRetry == null ? (Func<Exception, int, TimeSpan, Context, Task>)null : async (exception, i, timespan, ctx) => onRetry(exception, i, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync
            );
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx),
                sleepDurationProvider: sleepDurationProvider);
        }

        /// <summary>
        /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
        ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
        public static IAsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            return new AsyncRetryPolicy(
                policyBuilder,
                onRetryAsync: onRetryAsync == null ? (Func<Exception, TimeSpan, int, Context, Task>)null : (exception, timespan, i, ctx) => onRetryAsync(exception, i, timespan, ctx),
                sleepDurationProvider: sleepDurationProvider
            );
        }
    }
}

