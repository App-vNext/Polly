using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Polly.RateLimiting;

namespace Polly;

/// <summary>
/// The rate limiter extensions for <see cref="CompositeStrategyBuilder"/>.
/// </summary>
public static class RateLimiterCompositeStrategyBuilderExtensions
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
    /// <exception cref="ArgumentException">Thrown when <paramref name="permitLimit"/> or <paramref name="queueLimit"/> is invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        int permitLimit,
        int queueLimit = 0)
        where TBuilder : CompositeStrategyBuilderBase
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
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        ConcurrencyLimiterOptions options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            DefaultRateLimiterOptions = options
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
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = ResilienceRateLimiter.Create(limiter),
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
    /// <exception cref="ArgumentException">Thrown when <see cref="RateLimiterStrategyOptions.DefaultRateLimiterOptions"/> for <paramref name="options"/> are invalid.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RateLimiterStrategyOptions))]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members are preserved.")]
    public static TBuilder AddRateLimiter<TBuilder>(
        this TBuilder builder,
        RateLimiterStrategyOptions options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(
            context =>
            {
                return new RateLimiterResilienceStrategy(
                    options.RateLimiter ?? ResilienceRateLimiter.Create(new ConcurrencyLimiter(options.DefaultRateLimiterOptions)),
                    options.OnRejected,
                    context.Telemetry);
            },
            options);
    }
}
