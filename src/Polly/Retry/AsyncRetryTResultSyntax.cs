namespace Polly;

/// <summary>
///     Fluent API for defining an <see cref="AsyncRetryPolicy{TResult}" />.
/// </summary>
public static class AsyncRetryTResultSyntax
{
    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.RetryAsync(1);

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount)
    {
        Action<DelegateResult<TResult>, int> doNothing = (_, _) => { };

        return policyBuilder.RetryAsync(retryCount, doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry once
    ///     calling <paramref name="onRetry" /> on retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry) =>
#pragma warning disable 1998 // async method has no awaits, will run synchronously
        policyBuilder.RetryAsync(1, onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry once
    ///     calling <paramref name="onRetryAsync" /> on retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Task> onRetryAsync) =>
        policyBuilder.RetryAsync(1, onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryAsync(retryCount,
            onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.RetryAsync(retryCount, onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once
    /// calling <paramref name="onRetry"/> on retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry) =>
        policyBuilder.RetryAsync(1, onRetry);

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry once
    /// calling <paramref name="onRetryAsync"/> on retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync) =>
        policyBuilder.RetryAsync(1, onRetryAsync);

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryAsync(retryCount,
            onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, i, ctx) => onRetryAsync(outcome, i, ctx),
            retryCount);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely until the action succeeds.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
    {
        Action<DelegateResult<TResult>> doNothing = _ => { };

        return policyBuilder.RetryForeverAsync(doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (DelegateResult<TResult> outcome, Context _) => onRetry(outcome));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, i, _) => onRetry(outcome, i));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.RetryForeverAsync(onRetryAsync: (DelegateResult<TResult> outcome, Context _) => onRetryAsync(outcome));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and retry count.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.RetryForeverAsync(onRetryAsync: (outcome, i, _) => onRetryAsync(outcome, i));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, ctx) => onRetry(outcome, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.RetryForeverAsync(
            onRetryAsync: async (outcome, i, ctx) => onRetry(outcome, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, _, ctx) => onRetryAsync(outcome, ctx));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and context data.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> RetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            (outcome, _, i, ctx) => onRetryAsync(outcome, i, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry <paramref name="retryCount"/> times.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider)
    {
        Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider, doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, _) => onRetry(outcome, span));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, span, _, _) => onRetryAsync(outcome, span));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        }

        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
            .Select(sleepDurationProvider);

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            onRetryAsync,
            retryCount,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, span, _, ctx) => onRetry(outcome, span, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            sleepDurationProvider,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        return policyBuilder.WaitAndRetryAsync(
            retryCount,
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), result of previous execution, and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int retryCount,
        Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");
        }

        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            onRetryAsync,
            retryCount,
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations)
    {
        Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryAsync(sleepDurations, doNothing);
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result and the current sleep duration.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, _, _) => onRetry(outcome, timespan));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result and the current sleep duration.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: (outcome, timespan, _, _) => onRetryAsync(outcome, timespan));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, _, ctx) => onRetry(outcome, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
    {
        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx));
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetry" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryAsync(
            sleepDurations,
            onRetryAsync: async (outcome, timespan, i, ctx) => onRetry(outcome, timespan, i, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    ///     Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
    ///     <paramref name="sleepDurations" />
    ///     calling <paramref name="onRetryAsync" /> on each retry with the handled exception or result, the current sleep duration, retry count, and context data.
    ///     On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurations"/> or <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
    {
        if (sleepDurations == null)
        {
            throw new ArgumentNullException(nameof(sleepDurations));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            onRetryAsync,
            sleepDurationsEnumerable: sleepDurations);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        Action<DelegateResult<TResult>, TimeSpan> doNothing = (_, _) => { };

        return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for a particular retry attempt.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        Action<DelegateResult<TResult>, TimeSpan, Context> doNothing = (_, _, _) => { };

        return policyBuilder.WaitAndRetryForeverAsync(sleepDurationProvider, doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (outcome, timespan, _) => onRetry(outcome, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan> onRetry)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (outcome, i, timespan, _) => onRetry(outcome, i, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (outcome, timespan, _) => onRetryAsync(outcome, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and retry count.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (retryCount, _) => sleepDurationProvider(retryCount),
            (outcome, i, timespan, _) => onRetryAsync(outcome, i, timespan));
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryForeverAsync(
            sleepDurationProvider,
            async (outcome, timespan, ctx) => onRetry(outcome, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetry"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetry">The action to call on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetry"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
    {
        if (onRetry == null)
        {
            throw new ArgumentNullException(nameof(onRetry));
        }

#pragma warning disable 1998 // async method has no awaits, will run synchronously
        return policyBuilder.WaitAndRetryForeverAsync(
            sleepDurationProvider,
            async (outcome, i, timespan, ctx) => onRetry(outcome, i, timespan, ctx));
#pragma warning restore 1998
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc) and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        return policyBuilder.WaitAndRetryForeverAsync(
            (i, _, ctx) => sleepDurationProvider(i, ctx),
            onRetryAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            (outcome, timespan, _, ctx) => onRetryAsync(outcome, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }

    /// <summary>
    /// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely until the action succeeds,
    /// calling <paramref name="onRetryAsync"/> on each retry with the handled exception or result, retry count and execution context.
    ///     On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    ///     the current retry number (1 for first retry, 2 for second etc), previous execution result and execution context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
    /// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
    /// <returns>The policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepDurationProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onRetryAsync"/> is <see langword="null"/>.</exception>
    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Context, Task> onRetryAsync)
    {
        if (sleepDurationProvider == null)
        {
            throw new ArgumentNullException(nameof(sleepDurationProvider));
        }

        if (onRetryAsync == null)
        {
            throw new ArgumentNullException(nameof(onRetryAsync));
        }

        return new AsyncRetryPolicy<TResult>(
            policyBuilder,
            (exception, timespan, i, ctx) => onRetryAsync(exception, i, timespan, ctx),
            sleepDurationProvider: sleepDurationProvider);
    }
}

