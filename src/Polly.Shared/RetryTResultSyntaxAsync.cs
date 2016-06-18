#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Retry;

namespace Polly
{
    /// <summary>
    ///     Fluent API for defining a Retry <see cref="Policy" />.
    /// </summary>
    public static class RetryTResultSyntaxAsync
    {
        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
        {
            return policyBuilder.RetryAsync(1);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount)
        {
            Action<DelegateResult<TResult>, int> doNothing = (_, __) => { };

            return policyBuilder.RetryAsync(retryCount, doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry once
        ///     calling <paramref name="onRetry" /> on retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry)
        {
            return policyBuilder.RetryAsync(1, onRetry);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry once
        ///     calling <paramref name="onRetryAsync" /> on retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(1, onRetryAsync);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int> onRetry)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithCount<TResult>(retryCount, async (outcome, i) => onRetry(outcome, i)),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates, 
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithCount<TResult>(retryCount, onRetryAsync),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once
        /// calling <paramref name="onRetry"/> on retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry)
        {
            return RetryAsync(policyBuilder, 1, onRetry);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once
        /// calling <paramref name="onRetryAsync"/> on retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
        {
            return RetryAsync(policyBuilder, 1, onRetryAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int, Context> onRetry)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithCount<TResult>(retryCount, async (outcome, i, c) => onRetry(outcome, i, c), context),
#pragma warning restore 1998
                    continueOnCapturedContext
                    ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithCount<TResult>(retryCount, onRetryAsync, context),
                    continueOnCapturedContext
                ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static RetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
        {
            Action<Exception> doNothing = _ => { };

            return policyBuilder.RetryForeverAsync(doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<Exception> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyState<TResult>(async outcome => onRetry(outcome.Exception)),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
                );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyState<TResult>(onRetryAsync),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, Context> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyState<TResult>(async (outcome, c) => onRetry(outcome, c), context),
    #pragma warning restore 1998
                    continueOnCapturedContext
                ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, Task> onRetryAsync)
        {
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyState<TResult>(onRetryAsync, context),
                    continueOnCapturedContext
                ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
        {
            Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);
            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan) => onRetry(outcome, timespan)),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan, ctx) => onRetry(outcome, timespan, ctx), context),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx), context),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync, context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync, context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations)
        {
            Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryAsync(sleepDurations, doNothing);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan) => onRetry(outcome, timespan)),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan, ctx) => onRetry(outcome, timespan, ctx), context),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx), context),
#pragma warning restore 1998
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync, context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will wait and retry as many times as there are provided
        ///     <paramref name="sleepDurations" />
        ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
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
        public static RetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleep<TResult>(sleepDurations, onRetryAsync, context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        public static RetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");

            Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleepDurationProvider<TResult>(sleepDurationProvider, async (outcome, timespan) => onRetry(outcome, timespan)),
#pragma warning restore 1998
                    continueOnCapturedContext
                ),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>
        public static RetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleepDurationProvider<TResult>(sleepDurationProvider, onRetryAsync),
                    continueOnCapturedContext
                ),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>        
        public static RetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
#pragma warning disable 1998
                    () => new RetryPolicyStateWithSleepDurationProvider<TResult>(sleepDurationProvider, async (outcome, timespan, ctx) => onRetry(outcome, timespan, ctx), context),
    #pragma warning restore 1998
                    continueOnCapturedContext
                ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetryAsync</exception>        
        public static RetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) =>
                  RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    () => new RetryPolicyStateWithSleepDurationProvider<TResult>(sleepDurationProvider, onRetryAsync, context),
                    continueOnCapturedContext
                ), 
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }
    }
}

#endif