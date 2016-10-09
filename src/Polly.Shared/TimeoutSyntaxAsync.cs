using System;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.Utilities;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy TimeoutAsync(int seconds)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.EmptyTask;

            return TimeoutAsync(() => TimeSpan.FromSeconds(seconds), doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="seconds">The number of seconds after which to timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy TimeoutAsync(int seconds,Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (onTimeoutAsync == null) throw new ArgumentNullException(nameof(onTimeoutAsync));

            return TimeoutAsync(() => TimeSpan.FromSeconds(seconds), onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy TimeoutAsync(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.EmptyTask;

            return TimeoutAsync(() => timeout, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy TimeoutAsync(TimeSpan timeout,Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
            if (onTimeoutAsync == null) throw new ArgumentNullException(nameof(onTimeoutAsync));

            return TimeoutAsync(() => timeout, onTimeoutAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <returns>The policy instance.</returns>
        public static TimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider)
        {
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.EmptyTask;

            return TimeoutAsync(timeoutProvider, doNothingAsync);
        }

        /// <summary>
        /// Builds a <see cref="Policy"/> that will wait asynchronously for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
        /// </summary>
        /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
        /// <param name="onTimeoutAsync">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action. 
        /// <remarks>The Task parameter will be null if the executed action responded co-operatively to cancellation before the policy timed it out.</remarks></param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="System.ArgumentNullException">timeoutProvider</exception>
        /// <exception cref="System.ArgumentNullException">onTimeoutAsync</exception>
        public static TimeoutPolicy TimeoutAsync(Func<TimeSpan> timeoutProvider, Func<Context, TimeSpan, Task, Task> onTimeoutAsync)
        {
            if (timeoutProvider == null) throw new ArgumentNullException(nameof(timeoutProvider));
            if (onTimeoutAsync == null) throw new ArgumentNullException(nameof(onTimeoutAsync));

            return new TimeoutPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => TimeoutEngine.ImplementationAsync(
                    async ct => { await action(ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context,
                    timeoutProvider,
                    onTimeoutAsync,
                    cancellationToken, 
                    continueOnCapturedContext)
                );
        }
    }
}
