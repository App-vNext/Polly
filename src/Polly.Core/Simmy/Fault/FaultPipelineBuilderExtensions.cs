using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Fault;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class FaultPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, double injectionRate, Func<Exception?> faultGenerator)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        builder.AddChaosFault(new FaultStrategyOptions
        {
            Enabled = true,
            InjectionRate = injectionRate,
            FaultGenerator = (_) => new ValueTask<Exception?>(faultGenerator())
        });
        return builder;
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, FaultStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(
            context => new FaultChaosStrategy(options, context.Telemetry),
            options);

        return builder;
    }
}
