using System.Threading.RateLimiting;
using Polly.RateLimiting;
using Polly.Strategy;
using Polly.Utils;

namespace Polly;

/// <summary>
/// The rate limiter extensions for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class RateLimiterResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds the concurrency limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="permitLimit">Maximum number of permits that can be leased concurrently.</param>
    /// <param name="queueLimit">Maximum number of permits that can be queued concurrently.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddConcurrencyLimiter(
        this ResilienceStrategyBuilder builder,
        int permitLimit,
        int queueLimit = 0)
    {
        Guard.NotNull(builder);

        return builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = permitLimit,
            QueueLimit = queueLimit
        });
    }

    /// <summary>
    /// Adds the concurrency limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddConcurrencyLimiter(
        this ResilienceStrategyBuilder builder,
        ConcurrencyLimiterOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = new ConcurrencyLimiter(options),
        });
    }

    /// <summary>
    /// Adds the concurrency limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <param name="onRejected">The callback that is raised when rate limiter is rejected.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddConcurrencyLimiter(
        this ResilienceStrategyBuilder builder,
        ConcurrencyLimiterOptions options,
        Action<OnRateLimiterRejectedArguments> onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);
        Guard.NotNull(onRejected);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = new ConcurrencyLimiter(options),
            OnRejected = new NoOutcomeEvent<OnRateLimiterRejectedArguments>().Register(onRejected)
        });
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddRateLimiter(
        this ResilienceStrategyBuilder builder,
        RateLimiter limiter)
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = limiter,
        });
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <param name="onRejected">The callback that is raised when rate limiter is rejected.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddRateLimiter(
        this ResilienceStrategyBuilder builder,
        RateLimiter limiter,
        Action<OnRateLimiterRejectedArguments> onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);
        Guard.NotNull(onRejected);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = limiter,
            OnRejected = new NoOutcomeEvent<OnRateLimiterRejectedArguments>().Register(onRejected)
        });
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The rate limiter strategy options.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddRateLimiter(
        this ResilienceStrategyBuilder builder,
        RateLimiterStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The rate limiter strategy options are invalid.");

        return builder.AddStrategy(context => new RateLimiterResilienceStrategy(options.RateLimiter!, options.OnRejected, context.Telemetry), options);
    }
}
