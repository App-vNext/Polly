using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static partial class OutcomeChaosPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="fault">The exception to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Exception fault)
    {
        Guard.NotNull(builder);

        builder.AddFaultCore(new FaultStrategyOptions
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            Fault = fault
        });
        return builder;
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder AddChaosFault(
        this ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Func<Exception?> faultGenerator)
    {
        Guard.NotNull(builder);

        builder.AddFaultCore(new FaultStrategyOptions
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            FaultGenerator = (_) => new ValueTask<Exception?>(Task.FromResult(faultGenerator()))
        });
        return builder;
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder AddChaosFault(this ResiliencePipelineBuilder builder, FaultStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddFaultCore(options);
        return builder;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static void AddFaultCore(this ResiliencePipelineBuilder builder, FaultStrategyOptions options)
    {
        builder.AddStrategy(context =>
            new OutcomeChaosStrategy<object>(
                options,
                context.Telemetry),
            options);
    }
}
