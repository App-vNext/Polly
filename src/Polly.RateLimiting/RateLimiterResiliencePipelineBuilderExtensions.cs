using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Polly.RateLimiting;

namespace Polly;

/// <summary>
/// Extensions for adding rate limiting to <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class RateLimiterResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds the concurrency limiter.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="permitLimit">Maximum number of permits that can be leased concurrently.</param>
    /// <param name="queueLimit">Maximum number of permits that can be queued concurrently.</param>
    /// <returns>The builder instance with the concurrency limiter added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="permitLimit"/> or <paramref name="queueLimit"/> is invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        int permitLimit,
        int queueLimit = 0)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        return builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = permitLimit,
            QueueLimit = queueLimit
        });
    }

    /// <summary>
    /// Adds the concurrency limiter.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The concurrency limiter options.</param>
    /// <returns>The builder instance with the concurrency limiter added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddConcurrencyLimiter<TBuilder>(
        this TBuilder builder,
        ConcurrencyLimiterOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            DefaultRateLimiterOptions = options
        });
    }

    /// <summary>
    /// Adds the rate limiter.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="limiter">The rate limiter to use.</param>
    /// <returns>The builder instance with the rate limiter added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="limiter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options constructed from the arguments are invalid.</exception>
    public static TBuilder AddRateLimiter<TBuilder>(
        this TBuilder builder,
        RateLimiter limiter)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(limiter);

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = args => limiter.AcquireAsync(cancellationToken: args.Context.CancellationToken),
        });
    }

    /// <summary>
    /// Adds the rate limiter.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The rate limiter options.</param>
    /// <returns>The builder instance with the rate limiter added.</returns>
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
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(
            context =>
            {
                DisposeWrapper? wrapper = default;
                var limiter = options.RateLimiter;
                if (limiter is null)
                {
                    var defaultLimiter = new ConcurrencyLimiter(options.DefaultRateLimiterOptions);
                    wrapper = new DisposeWrapper(defaultLimiter);
                    limiter = args => defaultLimiter.AcquireAsync(cancellationToken: args.Context.CancellationToken);
                }

                return new RateLimiterResilienceStrategy(
                    limiter,
                    options.OnRejected,
                    context.Telemetry,
                    wrapper);
            },
            options);
    }
}
