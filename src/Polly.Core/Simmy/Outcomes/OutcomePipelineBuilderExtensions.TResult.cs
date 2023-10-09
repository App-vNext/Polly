using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Outcomes;

namespace Polly.Simmy;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
internal static class OutcomePipelineBuilderExtensions
{
    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="result">The outcome to inject. For disposable outcomes use either the generator or the options overload.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, TResult result)
    {
        Guard.NotNull(builder);

        builder.AddOutcomeCore<TResult, OutcomeStrategyOptions<TResult>>(new OutcomeStrategyOptions<TResult>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) => new ValueTask<Outcome<TResult>?>(Task.FromResult<Outcome<TResult>?>(Outcome.FromResult(result)))
        });
        return builder;
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1] (inclusive).</param>
    /// <param name="outcomeGenerator">The outcome generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(
        this ResiliencePipelineBuilder<TResult> builder, bool enabled, double injectionRate, Func<TResult?> outcomeGenerator)
    {
        Guard.NotNull(builder);

        builder.AddOutcomeCore<TResult, OutcomeStrategyOptions<TResult>>(new OutcomeStrategyOptions<TResult>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) => new ValueTask<Outcome<TResult>?>(Task.FromResult<Outcome<TResult>?>(Outcome.FromResult(outcomeGenerator())))
        });
        return builder;
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The outcome strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResiliencePipelineBuilder<TResult> AddChaosResult<TResult>(this ResiliencePipelineBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddOutcomeCore<TResult, OutcomeStrategyOptions<TResult>>(options);
        return builder;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static void AddOutcomeCore<TResult, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this ResiliencePipelineBuilder<TResult> builder,
        OutcomeStrategyOptions<TResult> options)
    {
        builder.AddStrategy(
            context => new OutcomeChaosStrategy<TResult>(options, context.Telemetry),
            options);
    }
}
