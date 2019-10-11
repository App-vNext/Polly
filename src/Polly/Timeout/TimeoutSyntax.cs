﻿using Polly.Timeout;
using System;
using System.Threading.Tasks;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy Timeout(int seconds)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static TimeoutPolicy Timeout(int seconds, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(int seconds, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(int seconds, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            return Timeout(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };

            return Timeout(ctx => timeout, TimeoutStrategy.Optimistic, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };

            return Timeout(ctx => timeout, timeoutStrategy, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout(ctx => timeout, TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout(ctx => timeout, TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout(ctx => timeout, timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout(ctx => timeout, timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };
            return Timeout(ctx => timeoutProvider(), TimeoutStrategy.Optimistic, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };
            return Timeout(ctx => timeoutProvider(), timeoutStrategy, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout(ctx => timeoutProvider(), TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout(ctx => timeoutProvider(), TimeoutStrategy.Optimistic, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout(ctx => timeoutProvider(), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout(ctx => timeoutProvider(), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider)
        {
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };
            return Timeout(timeoutProvider, TimeoutStrategy.Optimistic, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            Action<Context, TimeSpan, Task, Exception> doNothing = (_, __, ___, ____) => { };
            return Timeout(timeoutProvider, timeoutStrategy, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout)
            => Timeout(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout)
            => Timeout(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            if (onTimeout == null) throw new ArgumentNullException(nameof(onTimeout));

            return Timeout(timeoutProvider, timeoutStrategy, (ctx, timeout, task, ex) => onTimeout(ctx, timeout, task));
        }

        /// <summary>
        /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="ArgumentNullException">onTimeout</exception>
        public static TimeoutPolicy Timeout(
            Func<Context, TimeSpan> timeoutProvider, 
            TimeoutStrategy timeoutStrategy, 
            Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));
            if (onTimeout == null) throw new ArgumentNullException(nameof(onTimeout));

            return new TimeoutPolicy(
                    timeoutProvider,
                    timeoutStrategy,
                    onTimeout);
        }
    }
}