using System;
using System.Collections.Generic;
using System.Linq;
using Polly.Retry;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Retry <see cref="Policy"/>. 
    /// </summary>
    public static class RetrySyntax
    {
        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry once.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static Policy Retry(this PolicyBuilder policyBuilder)
        {
            return policyBuilder.Retry(1);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The policy instance.</returns>
        public static Policy Retry(this PolicyBuilder policyBuilder, int retryCount)
        {
            Action<Exception, int> doNothing = (_, __) => { };

            return policyBuilder.Retry(retryCount, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static Policy Retry(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int> onRetry)
        {
            if (retryCount <= 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than zero.");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            Func<IRetryPolicyState> policyStateFactory = () => new RetryPolicyStateWithCount(retryCount, onRetry);
            return new Policy(action => RetryPolicy.Implementation(action, policyBuilder.ExceptionPredicates, policyStateFactory));
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <returns>The policy instance.</returns>
        public static Policy RetryForever(this PolicyBuilder policyBuilder)
        {
            Action<Exception> doNothing = _ => { };

            return policyBuilder.RetryForever(doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will retry indefinitely
        /// calling <paramref name="onRetry"/> on each retry.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">onRetry</exception>
        public static Policy RetryForever(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
        {
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            Func<IRetryPolicyState> policyStateFactory = () => new RetryPolicyState(onRetry);
            return new Policy(action => RetryPolicy.Implementation(action, policyBuilder.ExceptionPredicates, policyStateFactory));
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times.
        /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
        /// the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <returns>The policy instance.</returns>
        public static Policy WaitAndRetry(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
        {
            Action<Exception, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetry(retryCount, sleepDurationProvider, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times
        /// calling <paramref name="onRetry"/> on each retry.
        /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
        /// the current retry attempt allowing an exponentially increasing wait time (exponential backoff).
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">
        /// timeSpanProvider
        /// or
        /// onRetry
        /// </exception>
        public static Policy WaitAndRetry(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
        {
            if (retryCount <= 0) throw new ArgumentOutOfRangeException("retryCount", "Value must be greater than zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException("sleepDurationProvider");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            var sleepDurations = Enumerable.Range(1, retryCount)
                                           .Select(sleepDurationProvider);

            Func<IRetryPolicyState> policyStateFactory = () => new RetryPolicyStateWithSleep(sleepDurations, onRetry);
            return new Policy(action => RetryPolicy.Implementation(action, policyBuilder.ExceptionPredicates, policyStateFactory));
        }


        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
        /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <returns>The policy instance.</returns>
        public static Policy WaitAndRetry(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations)
        {
            Action<Exception, TimeSpan> doNothing = (_, __) => { };

            return policyBuilder.WaitAndRetry(sleepDurations, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
        /// calling <paramref name="onRetry"/> on each retry.
        /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
        /// <param name="onRetry">The action to call on each retry.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// sleepDurations
        /// or
        /// onRetry
        /// </exception>
        public static Policy WaitAndRetry(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry)
        {
            if (sleepDurations == null) throw new ArgumentNullException("sleepDurations");
            if (onRetry == null) throw new ArgumentNullException("onRetry");

            Func<IRetryPolicyState> policyStateFactory = () => new RetryPolicyStateWithSleep(sleepDurations, onRetry);
            return new Policy(action => RetryPolicy.Implementation(action, policyBuilder.ExceptionPredicates, policyStateFactory));
        }
    }
}