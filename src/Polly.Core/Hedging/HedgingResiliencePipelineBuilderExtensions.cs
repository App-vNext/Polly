using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly;

/// <summary>
/// Extensions for adding hedging to <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class HedgingResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a hedging with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="options">The options to configure the hedging.</param>
    /// <returns>The builder instance with the hedging added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddHedging<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        HedgingStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(context => CreateHedgingStrategy(context, options), options);
    }

    private static HedgingResilienceStrategy<TResult> CreateHedgingStrategy<TResult>(StrategyBuilderContext context, HedgingStrategyOptions<TResult> options)
    {
        var handler = new HedgingHandler<TResult>(options.ShouldHandle!, options.ActionGenerator);

        return new HedgingResilienceStrategy<TResult>(
            options.Delay,
            options.MaxHedgedAttempts,
            handler,
            options.OnHedging,
            options.DelayGenerator,
            context.TimeProvider,
            context.Telemetry);
    }
}
