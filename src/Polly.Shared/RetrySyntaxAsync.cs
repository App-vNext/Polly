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
    public static partial class RetrySyntax
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
            Action<Exception, int> doNothing = (_, __) => { };

            return policyBuilder.RetryAsync(retryCount, doNothing);
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
            return policyBuilder.RetryAsync(1, onRetry);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry once
        ///     calling <paramref name="onRetryAsync" /> on retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync)
        {
            return policyBuilder.RetryAsync(1, onRetryAsync);
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
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithCount(retryCount,
                        async (e, i) => { onRetry.Invoke(e, i); await Task.FromResult(true).ConfigureAwait(continueOnCapturedContext); }
                    ),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry <paramref name="retryCount" /> times
        ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Task> onRetryAsync)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken, 
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithCount(retryCount, onRetryAsync), 
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
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
            return RetryAsync(policyBuilder, 1, onRetry);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once
        /// calling <paramref name="onRetryAsync"/> on retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetryAsync">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync)
        {
            return RetryAsync(policyBuilder, 1, onRetryAsync);
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
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy((action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                action,
                cancellationToken,
                policyBuilder.ExceptionPredicates,
                () => new RetryPolicyStateWithCount(retryCount,
                 async (e, i, c) => { onRetry.Invoke(e, i, c); await Task.FromResult(true).ConfigureAwait(continueOnCapturedContext); }
                , context),
                continueOnCapturedContext
            ), policyBuilder.ExceptionPredicates);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetryAsync">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (onRetryAsync == null) throw new ArgumentNullException("onRetryAsync");

            return new RetryPolicy((action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                action, 
                cancellationToken,
                policyBuilder.ExceptionPredicates,
                () => new RetryPolicyStateWithCount(retryCount, onRetryAsync, context),
                continueOnCapturedContext
            ), policyBuilder.ExceptionPredicates);
        }

        /// <summary>
        ///     Builds a <see cref="Policy" /> that will retry indefinitely.
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
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken, 
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyState(onRetry), 
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
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
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy((action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                action, 
                cancellationToken,
                policyBuilder.ExceptionPredicates,
                () => new RetryPolicyState(onRetry, context),
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
        ///     timeSpanProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken, 
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithSleep(sleepDurations, onRetry), 
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
        ///     timeSpanProvider
        ///     or
        ///     onRetry
        /// </exception>
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
            Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (retryCount < 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
                .Select(sleepDurationProvider);

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithSleep(sleepDurations, onRetry, context),
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
            Action<Exception, TimeSpan> doNothing = (_, __) => { };

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
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations,
            Action<Exception, TimeSpan> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken, 
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithSleep(sleepDurations, onRetry), 
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
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
        public static RetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations,
            Action<Exception, TimeSpan, Context> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithSleep(sleepDurations, onRetry, context),
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");

            Action<Exception, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => RetryEngine.ImplementationAsync(
                    action,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    () => new RetryPolicyStateWithSleepDurationProvider(sleepDurationProvider, onRetry),
                    continueOnCapturedContext
                ),
                policyBuilder.ExceptionPredicates
            );
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry with the raised exception and
        /// execution context.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurationProvider"></param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">sleepDurationProvider</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>        
        public static RetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
        {
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            return new RetryPolicy(
                (action, context, cancellationToken, continueOnCapturedContext)  => RetryEngine.ImplementationAsync(
                action,
                cancellationToken,
                policyBuilder.ExceptionPredicates,
                () => new RetryPolicyStateWithSleepDurationProvider(sleepDurationProvider, onRetry, context),
                continueOnCapturedContext
            ), policyBuilder.ExceptionPredicates);
        }
    }
}

#endif