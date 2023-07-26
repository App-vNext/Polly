using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly;

/// <summary>
/// Provides extension methods for configuring hedging resilience strategies for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class HedgingResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a hedging resilience strategy with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the hedging resilience strategy.</param>
    /// <returns>The builder instance with the hedging strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder<TResult> AddHedging<TResult>(this ResilienceStrategyBuilder<TResult> builder, HedgingStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddHedgingCore<TResult, HedgingStrategyOptions<TResult>>(options, isGeneric: true);
        return builder;
    }

    /// <summary>
    /// Adds a hedging resilience strategy with the provided options to the builder.
    /// </summary>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the hedging resilience strategy.</param>
    /// <returns>The builder instance with the hedging strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    internal static ResilienceStrategyBuilder AddHedging(this ResilienceStrategyBuilder builder, HedgingStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddHedgingCore<object, HedgingStrategyOptions>(options, isGeneric: false);
        return builder;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    internal static void AddHedgingCore<TResult, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this ResilienceStrategyBuilderBase builder,
        HedgingStrategyOptions<TResult> options,
        bool isGeneric)
    {
        builder.AddStrategy(context =>
        {
            var handler = new HedgingHandler<TResult>(
                options.ShouldHandle!,
                options.HedgingActionGenerator,
                isGeneric);

            return new HedgingResilienceStrategy<TResult>(
                options.HedgingDelay,
                options.MaxHedgedAttempts,
                handler,
                options.OnHedging,
                options.HedgingDelayGenerator,
                context.TimeProvider,
                context.Telemetry);
        },
        options);
    }
}
