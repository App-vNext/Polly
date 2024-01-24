using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding chaos outcome strategy to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class ChaosOutcomePipelineBuilderExtensions
{
    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="resultGenerator">The outcome generator delegate.</param>
    /// <returns>The same builder instance.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosOutcome<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        double injectionRate,
        Func<TResult?> resultGenerator)
    {
        Guard.NotNull(builder);

        builder.AddChaosOutcome(new ChaosOutcomeStrategyOptions<TResult>
        {
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) =>
            {
                return new ValueTask<Outcome<TResult>?>(Outcome.FromResult(resultGenerator()));
            }
        });
        return builder;
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The strategy options.</param>
    /// <returns>The same builder instance.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddChaosOutcome<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        ChaosOutcomeStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(
            context => new ChaosOutcomeStrategy<TResult>(options, context.Telemetry),
            options);

        return builder;
    }
}
