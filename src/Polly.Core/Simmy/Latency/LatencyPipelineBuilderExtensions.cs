﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Latency;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding latency to a <see cref="ResiliencePipelineBuilderBase"/>.
/// </summary>
internal static class LatencyPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a latency chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="latency">The delay value.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static TBuilder AddChaosLatency<TBuilder>(this TBuilder builder, bool enabled, double injectionRate, TimeSpan latency)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddChaosLatency(new LatencyStrategyOptions
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            Latency = latency
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
    public static TBuilder AddChaosLatency<TBuilder>(this TBuilder builder, LatencyStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(context => new LatencyChaosStrategy(options, context.TimeProvider, context.Telemetry), options);
    }
}

