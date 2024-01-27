using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Fault;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding chaos fault strategy to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class ChaosFaultPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The same builder instance.</returns>
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, double injectionRate, Func<Exception?> faultGenerator)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddChaosFault(new ChaosFaultStrategyOptions
        {
            InjectionRate = injectionRate,
            FaultGenerator = (_) => new ValueTask<Exception?>(faultGenerator())
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The same builder instance.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, ChaosFaultStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(
            context => new ChaosFaultStrategy(options, context.Telemetry),
            options);
    }
}
