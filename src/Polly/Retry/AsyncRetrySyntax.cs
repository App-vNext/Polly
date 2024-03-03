namespace Polly;

/// <summary>
///     Fluent API for defining a <see cref="AsyncRetryPolicy" />.
/// </summary>
public static class AsyncRetrySyntax
{
    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder) =>
        policyBuilder.RetryAsync(1);

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount)
    {
        Action<Exception, int, Context> doNothing = (_, _, _) => { };

        return policyBuilder.RetryAsync(retryCount, onRetry: doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry once
    ///     calling <paramref name="onRetry" /> on retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int> onRetry) =>
#pragma warning disable 1998 // async method has no awaits, will run synchronously
        policyBuilder.RetryAsync(1,
            onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry once
    ///     calling <paramref name="onRetryAsync" /> on retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync) =>
        policyBuilder.RetryAsync(1, onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));
#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryAsync(retryCount,
            onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.RetryAsync(retryCount, onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once
    /// calling <paramref name="onRetry"/> on retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Action<Exception, int, Context> onRetry) =>
        policyBuilder.RetryAsync(1, onRetry);

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry once
    /// calling <paramref name="onRetryAsync"/> on retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync) =>
        policyBuilder.RetryAsync(1, onRetryAsync);

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Action<Exception, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryAsync(retryCount,
            onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<Exception, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            (outcome, _, i, ctx) => onRetryAsync(outcome, i, ctx),
            retryCount);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely until the action succeeds.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder)
    {
        Action<Exception> doNothing = _ => { };

        return policyBuilder.RetryForeverAsync(doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (Exception outcome, Context _) => onRetry(outcome));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, int> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.RetryForeverAsync(onRetryAsync: (Exception outcome, Context _) => onRetryAsync(outcome));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and retry count.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.RetryForeverAsync(onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, ctx) => onRetry(outcome, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Action<Exception, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            (outcome, _, _, ctx) => onRetryAsync(outcome, ctx));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy RetryForeverAsync(this PolicyBuilder policyBuilder, Func<Exception, int, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            (outcome, _, i, ctx) => onRetryAsync(outcome, i, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry <paramref name="retryCount"/> times.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
    {
        Action<Exception, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, _) => onRetry(outcome, span));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, span, _, _) => onRetryAsync(outcome, span));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
            .Select(sleepDurationProvider);

        return new AsyncRetryPolicy(
            policyBuilder,
            onRetryAsync,
            retryCount,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, int retryCount,
        Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            onRetryAsync,
            retryCount,
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations)
    {
        Action<Exception, TimeSpan, int, Context> doNothing = (_, _, _, _) => { };

        return policyBuilder.WaitAndRetryAsync(sleepDurations, doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception and the current sleep duration.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, _, _) => onRetry(outcome, timespan));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception and the current sleep duration.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: (outcome, timespan, _, _) => onRetryAsync(outcome, timespan));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, _, ctx) => onRetry(outcome, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryAsync(this PolicyBuilder policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (sleepDurations == null)
            throw new ArgumentNullException(nameof(sleepDurations));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            onRetryAsync,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        Action<Exception, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        Action<Exception, TimeSpan, Context> doNothing = (_, _, _) => { };

        return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (exception, timespan, _) => onRetry(exception, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (exception, i, timespan, _) => onRetry(exception, i, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryForeverAsync(
         (retryCount, _) => sleepDurationProvider(retryCount),
         (exception, timespan, _) => onRetryAsync(exception, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and retry count.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc)
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (exception, i, timespan, _) => onRetryAsync(exception, i, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryForeverAsync(
            sleepDurationProvider,
            async (exception, timespan, ctx) => onRetry(exception, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the raised exception, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan, Context> onRetry)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetry == null)
            throw new ArgumentNullException(nameof(onRetry));

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryForeverAsync(
            sleepDurationProvider,
            async (exception, i, timespan, ctx) => onRetry(exception, i, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        return policyBuilder.WaitAndRetryForeverAsync(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));

        return policyBuilder.WaitAndRetryForeverAsync(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy WaitAndRetryForeverAsync(this PolicyBuilder policyBuilder, Func<int, Exception, Context, TimeSpan> sleepDurationProvider, Func<Exception, int, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        if (onRetryAsync == null)
            throw new ArgumentNullException(nameof(onRetryAsync));

        return new AsyncRetryPolicy(
            policyBuilder,
            (exception, timespan, i, ctx) => onRetryAsync(exception, i, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }
}

