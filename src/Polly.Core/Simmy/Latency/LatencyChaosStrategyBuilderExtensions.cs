using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Latency;

/// <summary>
/// Extension methods for adding timeouts to a <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class LatencyChaosStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a latency chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="delay">The delay value.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static TBuilder AddLatency<TBuilder>(this TBuilder builder, TimeSpan delay)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddLatency(new LatencyStrategyOptions
        {
            LatencyGenerator = (_) => new(delay)
        });
    }

    /// <summary>
    /// Adds a latency chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The latency options.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddLatency<TBuilder>(this TBuilder builder, LatencyStrategyOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new LatencyChaosStrategy(options, context.TimeProvider, context.Telemetry), options);
        return builder;
    }
}
