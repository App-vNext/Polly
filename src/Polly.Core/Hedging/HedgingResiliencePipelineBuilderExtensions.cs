using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly;

/// <summary>
/// Provides extension methods for configuring hedging resilience strategies for <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class HedgingResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a hedging resilience strategy with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="options">The options to configure the hedging resilience strategy.</param>
    /// <returns>The builder instance with the hedging strategy added.</returns>
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

        return builder.AddStrategy(context => CreateHedgingStrategy(context, options, isGeneric: true), options);
    }

    /// <summary>
    /// Adds a hedging resilience strategy with the provided options to the builder.
    /// </summary>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="options">The options to configure the hedging resilience strategy.</param>
    /// <returns>The builder instance with the hedging strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HedgingStrategyOptions))]
    internal static ResiliencePipelineBuilder AddHedging(this ResiliencePipelineBuilder builder, HedgingStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(context => CreateHedgingStrategy(context, options, isGeneric: false), options);
    }

    private static HedgingResilienceStrategy<TResult> CreateHedgingStrategy<TResult>(
        StrategyBuilderContext context,
        HedgingStrategyOptions<TResult> options,
        bool isGeneric)
    {
        var handler = new HedgingHandler<TResult>(
                        options.ShouldHandle!,
                        options.HedgingActionGenerator,
                        IsGeneric: isGeneric);

        return new HedgingResilienceStrategy<TResult>(
            options.HedgingDelay,
            options.MaxHedgedAttempts,
            handler,
            options.OnHedging,
            options.HedgingDelayGenerator,
            context.TimeProvider,
            context.Telemetry);
    }
}
