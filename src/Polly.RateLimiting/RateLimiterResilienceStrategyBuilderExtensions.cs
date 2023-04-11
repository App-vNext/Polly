using System.Threading.RateLimiting;
using Polly.Builder;
using Polly.Utils;

namespace Polly.RateLimiting;

/// <summary>
/// The rate limiter extensions for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class RateLimiterResilienceStrategyBuilderExtensions
{
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
    /// <param name="onRejected">The action that is invoked when the execution is rejected by the rate limiter.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddConcurrencyLimiter(
        this ResilienceStrategyBuilder builder,
        ConcurrencyLimiterOptions options,
        Action onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);
        Guard.NotNull(onRejected);

        return builder.AddConcurrencyLimiter(options, rejected => rejected.Add(onRejected));
    }

    /// <summary>
    /// Adds the concurrency limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <param name="onRejected">The callbacks that configures the <see cref="OnRateLimiterRejectedEvent"/>.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddConcurrencyLimiter(
        this ResilienceStrategyBuilder builder,
        ConcurrencyLimiterOptions options,
        Action<OnRateLimiterRejectedEvent> onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);
        Guard.NotNull(onRejected);

        var strategyOptions = new RateLimiterStrategyOptions
        {
            RateLimiter = new ConcurrencyLimiter(options)
        };
        onRejected(strategyOptions.OnRejected);

        return builder.AddRateLimiter(strategyOptions);
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
    /// <param name="onRejected">The action that is invoked when the execution is rejected by the rate limiter.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddRateLimiter(
        this ResilienceStrategyBuilder builder,
        RateLimiter limiter,
        Action onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);
        Guard.NotNull(onRejected);

        return builder.AddRateLimiter(limiter, rejected => rejected.Add(onRejected));
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <param name="onRejected">The callbacks that configures the <see cref="OnRateLimiterRejectedEvent"/>.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    public static ResilienceStrategyBuilder AddRateLimiter(
        this ResilienceStrategyBuilder builder,
        RateLimiter limiter,
        Action<OnRateLimiterRejectedEvent> onRejected)
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);
        Guard.NotNull(onRejected);

        var options = new RateLimiterStrategyOptions
        {
            RateLimiter = limiter,
        };

        onRejected(options.OnRejected);

        return builder.AddRateLimiter(options);
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
