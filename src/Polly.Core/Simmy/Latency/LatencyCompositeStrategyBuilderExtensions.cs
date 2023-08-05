using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly.Simmy.Latency;

/// <summary>
/// Extension methods for adding latency to a <see cref="CompositeStrategyBuilderBase"/>.
/// </summary>
public static class LatencyCompositeStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a latency chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="delay">The delay value.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static TBuilder AddLatency<TBuilder>(this TBuilder builder, bool enabled, double injectionRate, TimeSpan delay)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddLatency(new LatencyStrategyOptions
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
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
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static TBuilder AddLatency<TBuilder>(this TBuilder builder, LatencyStrategyOptions options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new LatencyChaosStrategy(options, context.TimeProvider, context.Telemetry), options);
        return builder;
    }
}

