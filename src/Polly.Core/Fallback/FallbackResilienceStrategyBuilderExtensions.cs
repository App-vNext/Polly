using System.ComponentModel.DataAnnotations;
using Polly.Fallback;

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="shouldHandle"/> or <paramref name="fallbackAction"/> is <see langword="null"/>.</exception>
    public static ResilienceStrategyBuilder<TResult> AddFallback<TResult>(
        this ResilienceStrategyBuilder<TResult> builder,
        Action<PredicateBuilder<TResult>> shouldHandle,
        Func<OutcomeArguments<TResult, HandleFallbackArguments>, ValueTask<TResult>> fallbackAction)
    {
        Guard.NotNull(builder);
        Guard.NotNull(shouldHandle);
        Guard.NotNull(fallbackAction);

        var options = new FallbackStrategyOptions<TResult>
        {
            FallbackAction = fallbackAction,
        };

        var predicateBuilder = new PredicateBuilder<TResult>();
        shouldHandle(predicateBuilder);

        options.ShouldHandle = predicateBuilder.CreatePredicate<HandleFallbackArguments>();

        return builder.AddFallback(options);
    }

    /// <summary>
    /// Adds a fallback resilience strategy with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the fallback resilience strategy.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder<TResult> AddFallback<TResult>(this ResilienceStrategyBuilder<TResult> builder, FallbackStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The fallback strategy options are invalid.");

        var handler = new FallbackHandler()
            .SetFallback<TResult>(handler =>
            {
                handler.FallbackAction = options.FallbackAction;
                handler.ShouldHandle = options.ShouldHandle;
            })
            .CreateHandler();

        builder.AddStrategy(context =>
            new FallbackResilienceStrategy(
                handler,
                context.CreateInvoker(options.OnFallback),
                context.Telemetry),
            options);

        return builder;
    }

    /// <summary>
    /// Adds a fallback resilience strategy with the provided options to the builder.
    /// </summary>
    /// <param name="builder">The resilience strategy builder.</param>
    /// <param name="options">The options to configure the fallback resilience strategy.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    internal static ResilienceStrategyBuilder AddFallback(this ResilienceStrategyBuilder builder, FallbackStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The fallback strategy options are invalid.");

        builder.AddStrategy(context =>
            new FallbackResilienceStrategy(
                options.Handler.CreateHandler(),
                context.CreateInvoker(options.OnFallback),
                context.Telemetry),
            options);

        return builder;
    }
}
