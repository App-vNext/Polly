using System;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.Utilities;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds, Func<Context
            , TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentNullException">seconds;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(int seconds, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return TimeoutAsync(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return TimeoutAsync(ctx => timeout, timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return TimeoutAsync(ctx => timeoutProvider(), timeoutStrategy, onTimeoutAsync);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<Context, TimeSpan> timeoutProvider)
        {
            return TimeoutAsync(timeoutProvider, DefaultTimeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            return TimeoutAsync(timeoutProvider, timeoutStrategy, onTimeoutAsync: (Func<Context, TimeSpan, Task, Exception, Task>)null);
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<Context, TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
            => TimeoutAsync(timeoutProvider, DefaultTimeoutStrategy, onTimeoutAsync);

        /// <summary>
        /// Builds an <see cref="AsyncPolicy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<Context, TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
            => TimeoutAsync(timeoutProvider, DefaultTimeoutStrategy, onTimeoutAsync);

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            return TimeoutAsync(timeoutProvider, timeoutStrategy, onTimeoutAsync == null ? (Func<Context, TimeSpan, Task, Exception, Task>)null : (ctx, timeout, task, ex) => onTimeoutAsync(ctx, timeout, task));
        }

        /// <summary>
        /// Builds an <see cref="AsyncPolicy" /> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static IAsyncTimeoutPolicy TimeoutAsync(
            Func<Context, TimeSpan> timeoutProvider, 
            TimeoutStrategy timeoutStrategy, 
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return new AsyncTimeoutPolicy(
                    timeoutProvider,
                    timeoutStrategy,
                    onTimeoutAsync
                );
        }
    }
}
