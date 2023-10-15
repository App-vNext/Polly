using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Fault;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
internal static partial class FaultPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="fault">The exception to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(this ResiliencePipelineBuilder<TResult> builder, double injectionRate, Exception fault)
    {
        builder.AddChaosFault(new FaultStrategyOptions
        {
            Enabled = true,
            InjectionRate = injectionRate,
            Fault = fault
        });
        return builder;
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(
        this ResiliencePipelineBuilder<TResult> builder, double injectionRate, Func<Exception?> faultGenerator)
    {
        builder.AddChaosFault(new FaultStrategyOptions
        {
            Enabled = true,
            InjectionRate = injectionRate,
            FaultGenerator = (_) => new ValueTask<Exception?>(Task.FromResult(faultGenerator()))
        });
        return builder;
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        FaultStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(
            context => new FaultChaosStrategy<TResult>(options, context.Telemetry),
            options);

        return builder;
    }
}
