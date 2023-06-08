using System.ComponentModel.DataAnnotations;
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

        ValidationHelper.ValidateObject(options, "The hedging strategy options are invalid.");

        builder.AddStrategy(context =>
        {
            var handler = new HedgingHandler<TResult>(
                context.CreateInvoker(options.ShouldHandle)!,
                options.HedgingActionGenerator,
                IsGeneric: true);

            return new HedgingResilienceStrategy<TResult>(
                options.HedgingDelay,
                options.MaxHedgedAttempts,
                handler,
                context.CreateInvoker(options.OnHedging),
                options.HedgingDelayGenerator,
                context.TimeProvider,
                context.Telemetry);
        },
        options);

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

        ValidationHelper.ValidateObject(options, "The hedging strategy options are invalid.");

        builder.AddStrategy(context =>
        {
            var handler = new HedgingHandler<object>(
                context.CreateInvoker(options.ShouldHandle)!,
                options.HedgingActionGenerator,
                IsGeneric: false);

            return new HedgingResilienceStrategy<object>(
                options.HedgingDelay,
                options.MaxHedgedAttempts,
                handler,
                context.CreateInvoker(options.OnHedging),
                options.HedgingDelayGenerator,
                context.TimeProvider,
                context.Telemetry);
        },
        options);

        return builder;
    }
}
