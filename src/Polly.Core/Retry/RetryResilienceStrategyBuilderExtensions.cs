using System;
using Polly.Builder;

namespace Polly.Retry;

/// <summary>
/// Retry extension methods for the <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class RetryResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddRetry(
        this ResilienceStrategyBuilder builder,
        Action<ShouldRetryPredicate> shouldRetry)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions();
        shouldRetry(options.ShouldRetry);

        return builder.AddRetry(options);
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <param name="backoffType">The backoff type to use for the retry strategy.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddRetry(
        this ResilienceStrategyBuilder builder,
        Action<ShouldRetryPredicate> shouldRetry,
        RetryBackoffType backoffType)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions
        {
            BackoffType = backoffType,
            RetryCount = RetryConstants.DefaultRetryCount,
            BaseDelay = RetryConstants.DefaultBaseDelay
        };

        shouldRetry(options.ShouldRetry);

        return builder.AddRetry(options);
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="shouldRetry">A predicate that defines the retry conditions.</param>
    /// <param name="backoffType">The backoff type to use for the retry strategy.</param>
    /// <param name="retryCount">The number of retries to attempt before giving up.</param>
    /// <param name="baseDelay">The base delay between retries.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddRetry(
        this ResilienceStrategyBuilder builder,
        Action<ShouldRetryPredicate> shouldRetry,
        RetryBackoffType backoffType,
        int retryCount,
        TimeSpan baseDelay)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldRetry);

        var options = new RetryStrategyOptions
        {
            BackoffType = backoffType,
            RetryCount = retryCount,
            BaseDelay = baseDelay
        };

        shouldRetry(options.ShouldRetry);

        return builder.AddRetry(options);
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddRetry(this ResilienceStrategyBuilder builder, RetryStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The retry strategy options are invalid.");

        return builder.AddStrategy(context => new RetryResilienceStrategy(options, context.TimeProvider, context.Telemetry));
    }
}
