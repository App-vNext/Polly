using System.ComponentModel.DataAnnotations;
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
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="permitLimit">Maximum number of permits that can be leased concurrently.</param>
    /// <param name="queueLimit">Maximum number of permits that can be queued concurrently.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        int permitLimit,
        int queueLimit = 0)
        where TBuilder : ResilienceStrategyBuilderBase
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
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        ConcurrencyLimiterOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
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
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <param name="onRejected">The callback that is raised when rate limiter is rejected.</param>
    /// <returns>The builder instance with the concurrency limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="options"/> or <paramref name="onRejected"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        ConcurrencyLimiterOptions options,
        Action<OnRateLimiterRejectedArguments> onRejected)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);
        Guard.NotNull(onRejected);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = new ConcurrencyLimiter(options),
            OnRejected = args =>
            {
                onRejected(args);
                return default;
            }
        });
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="limiter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddRateLimiter<TBuilder>(
        this TBuilder builder,
        RateLimiter limiter)
        where TBuilder : ResilienceStrategyBuilderBase
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
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <param name="onRejected">The callback that is raised when rate limiter is rejected.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddRateLimiter<TBuilder>(
        this TBuilder builder,
        RateLimiter limiter,
        Action<OnRateLimiterRejectedArguments> onRejected)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);
        Guard.NotNull(onRejected);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = limiter,
            OnRejected = args =>
            {
                onRejected(args);
                return default;
            }
        });
    }

    /// <summary>
    /// Adds the rate limiter strategy.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The rate limiter strategy options.</param>
    /// <returns>The builder instance with the rate limiter strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddRateLimiter<TBuilder>(
        this TBuilder builder,
        RateLimiterStrategyOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The rate limiter strategy options are invalid.");

        builder.AddStrategy(context => new RateLimiterResilienceStrategy(options.RateLimiter!, options.OnRejected, context.Telemetry), options);
        return builder;
    }
}
