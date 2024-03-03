namespace Polly;

/// <summary>
/// Fluent API for defining a Retry <see cref="Policy{TResult}"/>.
/// </summary>
public static class RetryTResultSyntax
{
    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry once.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.Retry(1);

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry <paramref name="retryCount"/> times.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount)
    {
        Action<DelegateResult<TResult>, int> doNothing = (_, _) => { };

        return policyBuilder.Retry(retryCount, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry once
    /// calling <paramref name="onRetry"/> on retry with the handled exception or result and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry) =>
        policyBuilder.Retry(1, onRetry);

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int> onRetry)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.Retry(retryCount, (outcome, i, _) => onRetry(outcome, i));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry once
    /// calling <paramref name="onRetry"/> on retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry) =>
        policyBuilder.Retry(1, onRetry);

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> Retry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, i, ctx) => onRetry(outcome, i, ctx),
            retryCount);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry indefinitely until the action succeeds.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> RetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder)
    {
        Action<DelegateResult<TResult>> doNothing = _ => { };

        return policyBuilder.RetryForever(doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> RetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.RetryForever((DelegateResult<TResult> outcome, Context _) => onRetry(outcome));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> RetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.RetryForever((outcome, i, _) => onRetry(outcome, i));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> RetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, _, ctx) => onRetry(outcome, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> RetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, i, ctx) => onRetry(outcome, i, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
    {
        Action<DelegateResult<TResult>, TimeSpan, int, Context> doNothing = (_, _, _, _) => { };

        return policyBuilder.WaitAndRetry(retryCount, sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and the current sleep duration.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// sleepDurationProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(
            retryCount,
            sleepDurationProvider,
            (outcome, span, _, _) => onRetry(outcome, span));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// sleepDurationProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(
            retryCount,
            sleepDurationProvider,
            (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration, retry count, and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// timeSpanProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        var sleepDurations = Enumerable.Range(1, retryCount)
                                       .Select(sleepDurationProvider);

        return new RetryPolicy<TResult>(
            policyBuilder,
            onRetry,
            retryCount,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider)
    {
        Action<DelegateResult<TResult>, TimeSpan, int, Context> doNothing = (_, _, _, _) => { };

        return policyBuilder.WaitAndRetry(retryCount, sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// sleepDurationProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(
            retryCount,
            sleepDurationProvider,
            (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration, retry count, and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// timeSpanProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry) =>
        policyBuilder.WaitAndRetry(
            retryCount,
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetry);

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider)
    {
        Action<DelegateResult<TResult>, TimeSpan, int, Context> doNothing = (_, _, _, _) => { };

        return policyBuilder.WaitAndRetry(retryCount, sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// sleepDurationProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(
            retryCount,
            sleepDurationProvider,
            (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration, retry count, and context data.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    /// the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">
    /// timeSpanProvider
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            onRetry,
            retryCount,
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
    /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <returns>The policy instance.</returns>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations)
    {
        Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetry(sleepDurations, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and the current sleep duration.
    /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// sleepDurations
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(sleepDurations, (outcome, span, _, _) => onRetry(outcome, span));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration and context data.
    /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// sleepDurations
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetry(sleepDurations, (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, current sleep duration, retry count and context data.
    /// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// sleepDurations
    /// or
    /// onRetry
    /// </exception>
    public static RetryPolicy<TResult> WaitAndRetry<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (sleepDurations == null)
            throw new ArgumentNullException(nameof(sleepDurations));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            onRetry,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryForever(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        Action<DelegateResult<TResult>, TimeSpan, Context> doNothing = (_, _, _) => { };

        return policyBuilder.WaitAndRetryForever(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetryForever(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (exception, timespan, _) => onRetry(exception, timespan));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and retry count.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetryForever(
            (retryCount, _, _) => sleepDurationProvider(retryCount),
            (outcome, i, timespan, _) => onRetry(outcome, i, timespan));
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        return policyBuilder.WaitAndRetryForever(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetry);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        return policyBuilder.WaitAndRetryForever(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetry);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            (outcome, timespan, _, ctx) => onRetry(outcome, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    /// Builds a <see cref="Policy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static RetryPolicy<TResult> WaitAndRetryForever<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return new RetryPolicy<TResult>(
            policyBuilder,
            (exception, timespan, i, ctx) => onRetry(exception, i, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }
}
