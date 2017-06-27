using System;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.Utilities;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, TimeoutStrategy timeoutStrategy)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => timeout, TimeoutStrategy.Optimistic, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => timeout, timeoutStrategy, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
            if (onTimeoutAsync == null) throw new ArgumentNullException(nameof(onTimeoutAsync));

            return TimeoutAsync<TResult>(ctx => timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            return TimeoutAsync<TResult>(ctx => timeout, timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => timeoutProvider(), TimeoutStrategy.Optimistic, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(ctx => timeoutProvider(), timeoutStrategy, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), TimeoutStrategy.Optimistic, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider)
        {
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(timeoutProvider, TimeoutStrategy.Optimistic, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.FromResult(default(TResult));
            return TimeoutAsync<TResult>(timeoutProvider, timeoutStrategy, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            return TimeoutAsync<TResult>(timeoutProvider, TimeoutStrategy.Optimistic, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));
            if (onTimeoutAsync == null) throw new ArgumentNullException(nameof(onTimeoutAsync));

            return new TimeoutPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => TimeoutEngine.ImplementationAsync(
                    action,
                    context,
                    timeoutProvider,
                    timeoutStrategy,
                    onTimeoutAsync,
                    cancellationToken, 
                    continueOnCapturedContext)
                );
        }
    }
}
