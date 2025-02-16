namespace Polly;

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
        return Timeout(_ => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, EmptyHandler);
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
        return Timeout(_ => TimeSpan.FromSeconds(seconds), timeoutStrategy, EmptyHandler);
    }

    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="seconds">The number of seconds after which to timeout.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(int seconds, Action<Context, TimeSpan, Task> onTimeout)
    {
        TimeoutValidator.ValidateSecondsTimeout(seconds);

        return Timeout(_ => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="seconds">The number of seconds after which to timeout.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">seconds;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(int seconds, Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        return Timeout(_ => TimeSpan.FromSeconds(seconds), TimeoutStrategy.Optimistic, onTimeout);
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
    {
        TimeoutValidator.ValidateSecondsTimeout(seconds);

        return Timeout(_ => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(int seconds, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        return Timeout(_ => TimeSpan.FromSeconds(seconds), timeoutStrategy, onTimeout);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);
        return Timeout(_ => timeout, TimeoutStrategy.Optimistic, EmptyHandler);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);
        return Timeout(_ => timeout, timeoutStrategy, EmptyHandler);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task"/> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout, Action<Context, TimeSpan, Task> onTimeout)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);

        return Timeout(_ => timeout, TimeoutStrategy.Optimistic, onTimeout);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task"/> capturing the abandoned, timed-out action, and captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout, Action<Context, TimeSpan, Task, Exception> onTimeout)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);

        return Timeout(_ => timeout, TimeoutStrategy.Optimistic, onTimeout);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);

        return Timeout(_ => timeout, timeoutStrategy, onTimeout);
    }

#pragma warning disable S3872
    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">timeout;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(TimeSpan timeout, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
#pragma warning restore S3872
    {
        TimeoutValidator.ValidateTimeSpanTimeout(timeout);

        return Timeout(_ => timeout, timeoutStrategy, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <returns>The policy instance.</returns>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), TimeoutStrategy.Optimistic, EmptyHandler);
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), timeoutStrategy, EmptyHandler);
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), TimeoutStrategy.Optimistic, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), TimeoutStrategy.Optimistic, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), timeoutStrategy, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        return Timeout(_ => timeoutProvider(), timeoutStrategy, onTimeout);
    }

    /// <summary>
    /// Builds a <see cref="Policy"/> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException"/> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <returns>The policy instance.</returns>
    public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider)
        => Timeout(timeoutProvider, TimeoutStrategy.Optimistic, EmptyHandler);

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy)
        => Timeout(timeoutProvider, timeoutStrategy, EmptyHandler);

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task> onTimeout) =>
        Timeout(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, Action<Context, TimeSpan, Task, Exception> onTimeout) =>
        Timeout(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, and a <see cref="Task" /> capturing the abandoned, timed-out action.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(Func<Context, TimeSpan> timeoutProvider, TimeoutStrategy timeoutStrategy, Action<Context, TimeSpan, Task> onTimeout)
    {
        if (onTimeout == null)
        {
            throw new ArgumentNullException(nameof(onTimeout));
        }

        return Timeout(timeoutProvider, timeoutStrategy, (ctx, timeout, task, _) => onTimeout(ctx, timeout, task));
    }

    /// <summary>
    /// Builds a <see cref="Policy" /> that will wait for a delegate to complete for a specified period of time. A <see cref="TimeoutRejectedException" /> will be thrown if the delegate does not complete within the configured timeout.
    /// </summary>
    /// <param name="timeoutProvider">A function to provide the timeout for this execution.</param>
    /// <param name="timeoutStrategy">The timeout strategy.</param>
    /// <param name="onTimeout">An action to call on timeout, passing the execution context, the timeout applied, the <see cref="Task" /> capturing the abandoned, timed-out action, and the captured <see cref="Exception"/>.
    /// <remarks>The Task parameter will be null if the executed action responded cooperatively to cancellation before the policy timed it out.</remarks></param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeoutProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onTimeout"/> is <see langword="null"/>.</exception>
    public static TimeoutPolicy Timeout(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        if (timeoutProvider == null)
        {
            throw new ArgumentNullException(nameof(timeoutProvider));
        }

        if (onTimeout == null)
        {
            throw new ArgumentNullException(nameof(onTimeout));
        }

        return new TimeoutPolicy(
            timeoutProvider,
            timeoutStrategy,
            onTimeout);
    }

    private static void EmptyHandler(Context context, TimeSpan timeout, Task task, Exception exception)
    {
        // No-op
    }
}
