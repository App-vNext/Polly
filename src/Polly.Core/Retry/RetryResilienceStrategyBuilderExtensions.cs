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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(
        this ResilienceStrategyBuilder<TResult> builder,
        Action<PredicateBuilder<TResult>> shouldRetry)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions<TResult>();
        ConfigureShouldRetry(shouldRetry, options);

        return builder.AddRetry(options);
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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(
        this ResilienceStrategyBuilder<TResult> builder,
        Action<PredicateBuilder<TResult>> shouldRetry,
        Func<int, TimeSpan> retryDelayGenerator)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);
        Guard.NotNull(retryDelayGenerator);

        var options = new RetryStrategyOptions<TResult>();
        ConfigureShouldRetry(shouldRetry, options);
        options.RetryDelayGenerator = (_, args) => new ValueTask<TimeSpan>(retryDelayGenerator(args.Attempt));

        return builder.AddRetry(options);
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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(
        this ResilienceStrategyBuilder<TResult> builder,
        Action<PredicateBuilder<TResult>> shouldRetry,
        RetryBackoffType backoffType)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions<TResult>
        {
            BackoffType = backoffType,
            RetryCount = RetryConstants.DefaultRetryCount,
            BaseDelay = RetryConstants.DefaultBaseDelay
        };

        ConfigureShouldRetry(shouldRetry, options);

        return builder.AddRetry(options);
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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(
        this ResilienceStrategyBuilder<TResult> builder,
        Action<PredicateBuilder<TResult>> shouldRetry,
        RetryBackoffType backoffType,
        int retryCount,
        TimeSpan baseDelay)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions<TResult>
        {
            BackoffType = backoffType,
            RetryCount = retryCount,
            BaseDelay = baseDelay
        };

        ConfigureShouldRetry(shouldRetry, options);

        return builder.AddRetry(options);
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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(this ResilienceStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The retry strategy options are invalid.");

        return builder.AddStrategy(context =>
            new RetryResilienceStrategy(
                options.BaseDelay,
                options.BackoffType,
                options.RetryCount,
                context.CreateInvoker(options.ShouldRetry)!,
                context.CreateInvoker(options.OnRetry),
                context.CreateInvoker(options.RetryDelayGenerator, TimeSpan.MinValue),
                context.TimeProvider,
                context.Telemetry,
                RandomUtil.Instance),
            options);
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

        return builder.AddStrategy(context =>
            new RetryResilienceStrategy(
                options.BaseDelay,
                options.BackoffType,
                options.RetryCount,
                context.CreateInvoker(options.ShouldRetry)!,
                context.CreateInvoker(options.OnRetry),
                context.CreateInvoker(options.RetryDelayGenerator, TimeSpan.MinValue),
                context.TimeProvider,
                context.Telemetry,
                RandomUtil.Instance),
            options);
    }

    private static void ConfigureShouldRetry<TResult>(Action<PredicateBuilder<TResult>> shouldRetry, RetryStrategyOptions<TResult> options)
    {
        var predicate = new PredicateBuilder<TResult>();
        shouldRetry(predicate);
        options.ShouldRetry = predicate.CreatePredicate<ShouldRetryArguments>();
    }
}
