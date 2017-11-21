using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Retry;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    ///     Fluent API for defining a Retry <see cref="Policy" />.
    /// </summary>
    public static class RetrySyntaxAsync
    {
        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder)
        {
            return policyBuilder.RetryAsync(1);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount)
        {
            Action<Exception, int, Context> doNothing = (_, __, ___) => { };

            return policyBuilder.RetryAsync(retryCount, onRetry: doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry once
        ///     calling <paramref name="onRetry" /> on retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int> onRetry)
        {
            return policyBuilder.RetryAsync(1,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry once
        ///     calling <paramref name="onRetryAsync" /> on retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(1, onRetryAsync: (outcome, i, ctx) => onRetryAsync(outcome, i));
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.RetryAsync(retryCount, onRetryAsync: (outcome, i, ctx) => onRetryAsync(outcome, i));
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once
        /// calling <paramref name="onRetry"/> on retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int, Context> onRetry)
        {
            return policyBuilder.RetryAsync(1, onRetry);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once
        /// calling <paramref name="onRetryAsync"/> on retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(1, onRetryAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.RetryAsync(retryCount,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, 
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                () => new RetryStateRetryWithCount<EmptyStruct>(retryCount, (outcome, i, ctx) => onRetryAsync(outcome.Exception, i, ctx), context),
                continueOnCapturedContext
            ), policyBuilder.ExceptionPredicates);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely until the action succeeds.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder)
        {
            Action<Exception> doNothing = _ => { };

            return policyBuilder.RetryForeverAsync(doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, ctx) => onRetry(outcome)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.RetryForeverAsync(onRetryAsync: (outcome, ctx) => onRetryAsync(outcome));
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.RetryForeverAsync(
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, ctx) => onRetry(outcome, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Context, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, 
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                () => new RetryStateRetryForever<EmptyStruct>((outcome, ctx) => onRetryAsync(outcome.Exception, ctx), context),
                continueOnCapturedContext
            ), policyBuilder.ExceptionPredicates);
        }

        /// <summary>
        ///     Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
        {
            Action<Exception, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, span, i, ctx) => onRetry(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: (outcome, span, i, ctx) => onRetryAsync(outcome, span)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, 
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    () => new RetryStateWaitAndRetry<EmptyStruct>(sleepDurations, (outcome, timespan, i, ctx) => onRetryAsync(outcome.Exception, timespan, i, ctx), context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, span, i, ctx) => onRetry(outcome, span, ctx)
#pragma warning restore 1998
                );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetryAsync: (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryAsync(
                retryCount,
                (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
        ///     the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurationProvider
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    () => new RetryStateWaitAndRetryWithProvider<EmptyStruct>(
                        retryCount,
                        (i, outcome, ctx) => sleepDurationProvider(i, outcome.Exception, ctx),
                        (outcome, timespan, i, ctx) => onRetryAsync(outcome.Exception, timespan, i, ctx),
                        context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations)
        {
            Action<Exception, TimeSpan, int, Context> doNothing = (_, __, ___, ____) => { };

            return policyBuilder.WaitAndRetryAsync(sleepDurations, doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan)
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, ctx)
#pragma warning restore 1998
            );

        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: (outcome, timespan, i, ctx) => onRetryAsync(outcome, timespan, ctx)
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryAsync(
                sleepDurations,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
        ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     sleepDurations
        ///     or
        ///     onRetryAsync
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException(nameof(sleepDurations));
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, 
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    () => new RetryStateWaitAndRetry<EmptyStruct>(sleepDurations, (outcome, timespan, i, ctx) => onRetryAsync(outcome.Exception, timespan, i, ctx), context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            Action<Exception, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            Action<Exception, TimeSpan, Context> doNothing = (_, __, ___) => { };

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryForeverAsync(
                (retryCount, context) => sleepDurationProvider(retryCount),
                (exception, timespan, context) => onRetry(exception, timespan)
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return policyBuilder.WaitAndRetryForeverAsync(
             (retryCount, context) => sleepDurationProvider(retryCount),
             (exception, timespan, context) => onRetryAsync(exception, timespan)
         );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>        
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

            return policyBuilder.WaitAndRetryForeverAsync(
                sleepDurationProvider,
#pragma warning disable 1998 // async method has no awaits, will run synchronously
                async (exception, timespan, ctx) => onRetry(exception, timespan, ctx)
#pragma warning restore 1998
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>        
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            return policyBuilder.WaitAndRetryForeverAsync(
                (i, outcome, ctx) => sleepDurationProvider(i, ctx),
                onRetryAsync
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>        
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context, 
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    () => new RetryStateWaitAndRetryForever<EmptyStruct>(
                        (i, outcome, ctx) => sleepDurationProvider(i, outcome.Exception, ctx),
                        (outcome, timespan, ctx) => onRetryAsync(outcome.Exception, timespan, ctx), 
                        context),
                    continueOnCapturedContext
                  ), policyBuilder.ExceptionPredicates);
        }
    }
}

