using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly.Simmy.Behavior;

/// <summary>
/// Extension methods for adding custom behaviors to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class BehaviorResiliencePipelineBuilderExtensions

{
    /// <summary>
    /// Adds a behavior chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="behavior">The behavior to be injected.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static TBuilder AddBehavior<TBuilder>(this TBuilder builder, bool enabled, double injectionRate, Func<ValueTask> behavior)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);

        return builder.AddBehavior(new BehaviorStrategyOptions
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            BehaviorAction = (_) => behavior()
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
    public static TBuilder AddBehavior<TBuilder>(this TBuilder builder, BehaviorStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new BehaviorChaosStrategy(options, context.Telemetry), options);
        return builder;
    }
}
