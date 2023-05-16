using System;
using Polly.Fallback;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// Provides extension methods for configuring fallback resilience strategies for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class FallbackResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a fallback resilience strategy for a specific <typeparamref name="TResult"/> type to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="shouldHandle">An action to configure the fallback predicate.</param>
    /// <param name="fallbackAction">The fallback action to be executed.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    public static ResilienceStrategyBuilder AddFallback<TResult>(
        this ResilienceStrategyBuilder builder,
        Action<OutcomePredicate<HandleFallbackArguments, TResult>> shouldHandle,
        Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>> fallbackAction)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldHandle);
        Guard.NotNull(fallbackAction);

        var options = new FallbackStrategyOptions<TResult>
        {
            FallbackAction = fallbackAction,
        };

        shouldHandle(options.ShouldHandle);

        return builder.AddFallback(options);
    }

    /// <summary>
    /// Adds a fallback resilience strategy with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the fallback resilience strategy.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    public static ResilienceStrategyBuilder AddFallback<TResult>(this ResilienceStrategyBuilder builder, FallbackStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The fallback strategy options are invalid.");

        return builder.AddFallback(options.AsNonGenericOptions());
    }

    /// <summary>
    /// Adds a fallback resilience strategy with the provided options to the builder.
    /// </summary>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the fallback resilience strategy.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    public static ResilienceStrategyBuilder AddFallback(this ResilienceStrategyBuilder builder, FallbackStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The fallback strategy options are invalid.");

        return builder.AddStrategy(context => new FallbackResilienceStrategy(options, context.Telemetry), options);
    }
}
