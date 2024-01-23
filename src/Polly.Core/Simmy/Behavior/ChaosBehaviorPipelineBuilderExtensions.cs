using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Behavior;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding custom behaviors to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class ChaosBehaviorPipelineBuilderExtensions
{
    /// <summary>
    /// Adds a behavior chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="behavior">The behavior to be injected.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static TBuilder AddChaosBehavior<TBuilder>(this TBuilder builder, double injectionRate, Func<CancellationToken, ValueTask> behavior)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddChaosBehavior(new ChaosBehaviorStrategyOptions
        {
            Enabled = true,
            InjectionRate = injectionRate,
            BehaviorGenerator = args => behavior(args.Context.CancellationToken)
        });
    }

    /// <summary>
    /// Adds a behavior chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The behavior options.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static TBuilder AddChaosBehavior<TBuilder>(this TBuilder builder, ChaosBehaviorStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(context => new ChaosBehaviorStrategy(options, context.Telemetry), options);
    }
}
