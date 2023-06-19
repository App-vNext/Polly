using System.ComponentModel.DataAnnotations;
using Polly.Retry;

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
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry(this ResilienceStrategyBuilder builder, RetryStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddRetryCore(options);
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

        return builder.AddRetryCore(options);
    }

    private static TBuilder AddRetryCore<TBuilder, TResult>(this TBuilder builder, RetryStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(context =>
            new RetryResilienceStrategy(
                options.BaseDelay,
                options.BackoffType,
                options.RetryCount,
                context.CreateInvoker(options.ShouldHandle)!,
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
        options.ShouldHandle = predicate.CreatePredicate<RetryPredicateArguments>();
    }
}
