using System;
using System.Threading.Tasks;
using Polly.Timeout;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(int seconds, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync<TResult>(ctx => timeout, timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
            
            return TimeoutAsync<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync<TResult>(ctx => timeout, timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            return TimeoutAsync<TResult>(ctx => timeout, timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider)
        {
            return TimeoutAsync<TResult>(timeoutProvider, DefaultTimeoutStrategy, (Func<Context, TimeSpan, Task, Exception, Task>) null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            return TimeoutAsync<TResult>(timeoutProvider, timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
            => TimeoutAsync<TResult>(timeoutProvider, DefaultTimeoutStrategy, onTimeoutAsync);

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
            => TimeoutAsync<TResult>(timeoutProvider, DefaultTimeoutStrategy, onTimeoutAsync);

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            return TimeoutAsync<TResult>(timeoutProvider, timeoutStrategy, onTimeoutAsync == null ? (Func<Context, TimeSpan, Task, Exception, Task>)null : (ctx, timeout, task, ex) => onTimeoutAsync(ctx, timeout, task));
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy{TResult}"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy<TResult> TimeoutAsync<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return new AsyncTimeoutPolicy<TResult>(
                    timeoutProvider,
                    timeoutStrategy,
                    onTimeoutAsync
                );
        }
    }
}
