using System;
using Polly.Timeout;
using System.Threading.Tasks;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);
            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateSecondsTimeout(seconds);

            return Timeout<TResult>(ctx => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);

            return Timeout<TResult>(ctx => timeout, timeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            return Timeout<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            return Timeout<TResult>(ctx => timeout, DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            return Timeout<TResult>(ctx => timeout, timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeout">The timeout.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            TimeoutValidator.ValidateTimeSpanTimeout(timeout);
            return Timeout<TResult>(ctx => timeout, timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), DefaultTimeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return Timeout<TResult>(ctx => timeoutProvider(), timeoutStrategy, onTimeout);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<Context, TimeSpan> timeoutProvider)
        {
            return Timeout<TResult>(timeoutProvider, DefaultTimeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        {
            return Timeout<TResult>(timeoutProvider, timeoutStrategy, onTimeout: (Action<Context, TimeSpan, Task, Exception>)null);
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout)
            => Timeout<TResult>(timeoutProvider, DefaultTimeoutStrategy, onTimeout);

        /// <summary>
        /// Builds a <see cref="Policy{TResult}"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout)
            => Timeout<TResult>(timeoutProvider, DefaultTimeoutStrategy, onTimeout);

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task{TResult}" /> capturing the abandoned, timed-out action.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
        {
            return Timeout<TResult>(timeoutProvider, timeoutStrategy, onTimeout == null ? (Action<Context, TimeSpan, Task, Exception>)null : (ctx, timeout, task, ex) => onTimeout(ctx, timeout, task));
        }

        /// <summary>
        /// Builds a <see cref="Policy{TResult}" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="timeoutStrategy">The timeout strategy.</param>
        /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task{TResult}" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
        /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">timeoutProvider</exception>
        public static ISyncTimeoutPolicy<TResult> Timeout<TResult>(
            Func<Context, TimeSpan> timeoutProvider, 
            TimeoutStrategy timeoutStrategy, 
            Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));

            return new TimeoutPolicy<TResult>(
                    timeoutProvider,
                    timeoutStrategy,
                    onTimeout);
        }
    }
}