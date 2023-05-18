using System;
using System.ComponentModel.DataAnnotations;
using Polly.Retry;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// Retry extension methods for the <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class RetryResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="shouldRetry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry<TResult>(
        this ResilienceStrategyBuilder builder,
        Func<Outcome<TResult>, bool> shouldRetry)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        return builder.AddRetry(new RetryStrategyOptions<TResult>
        {
            ShouldRetry = (o, _) => new ValueTask<bool>(shouldRetry(o))
        });
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <param name="retryDelayGenerator">The generator for retry delays. The argument is zero-based attempt number.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="shouldRetry"/> or <paramref name="retryDelayGenerator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry<TResult>(
        this ResilienceStrategyBuilder builder,
        Func<Outcome, bool> shouldRetry,
        Func<int, TimeSpan> retryDelayGenerator)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);
        Guard.NotNull(retryDelayGenerator);

        return builder.AddRetry(new RetryStrategyOptions
        {
            ShouldRetry = (o, _) => new ValueTask<bool>(shouldRetry(o)),
            RetryDelayGenerator = (_, args) => new ValueTask<TimeSpan>(retryDelayGenerator(args.Attempt))
        });
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <param name="backoffType">The backoff type to use for the retry strategy.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="shouldRetry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry<TResult>(
        this ResilienceStrategyBuilder builder,
        Func<Outcome<TResult>, bool> shouldRetry,
        RetryBackoffType backoffType)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        return builder.AddRetry(new RetryStrategyOptions<TResult>
        {
            BackoffType = backoffType,
            RetryCount = RetryConstants.DefaultRetryCount,
            BaseDelay = RetryConstants.DefaultBaseDelay,
            ShouldRetry = (o, _) => new ValueTask<bool>(shouldRetry(o))
        });
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <param name="backoffType">The backoff type to use for the retry strategy.</param>
    /// <param name="retryCount">The number of retries to attempt before giving up.</param>
    /// <param name="baseDelay">The base delay between retries.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="shouldRetry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry<TResult>(
        this ResilienceStrategyBuilder builder,
        Func<Outcome<TResult>, bool> shouldRetry,
        RetryBackoffType backoffType,
        int retryCount,
        TimeSpan baseDelay)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        return builder.AddRetry(new RetryStrategyOptions<TResult>
        {
            BackoffType = backoffType,
            RetryCount = retryCount,
            BaseDelay = baseDelay,
            ShouldRetry = (o, _) => new ValueTask<bool>(shouldRetry(o))
        });
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry<TResult>(this ResilienceStrategyBuilder builder, RetryStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The retry strategy options are invalid.");

        return builder.AddRetry(options.AsNonGenericOptions());
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry(this ResilienceStrategyBuilder builder, RetryStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The retry strategy options are invalid.");

        return builder.AddStrategy(context => new RetryResilienceStrategy(options, context.TimeProvider, context.Telemetry, RandomUtil.Instance));
    }
}
