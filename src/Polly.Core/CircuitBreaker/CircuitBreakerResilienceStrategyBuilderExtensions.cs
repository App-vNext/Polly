using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// Circuit breaker strategy extensions for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class CircuitBreakerResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Add advanced circuit breaker strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="AdvancedCircuitBreakerStrategyOptions{TResult}"/> for more details about the advanced circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddAdvancedCircuitBreaker<TBuilder, TResult>(this TBuilder builder, AdvancedCircuitBreakerStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The advanced circuit breaker strategy options are invalid.");

        builder.AddStrategy(
            context =>
            {
                var behavior = new AdvancedCircuitBehavior(
                    options.FailureThreshold,
                    options.MinimumThroughput,
                    HealthMetrics.Create(options.SamplingDuration, context.TimeProvider));

                return CreateStrategy(context, options, behavior);
            },
            options);

        return builder;
    }

    /// <summary>
    /// Add simple circuit breaker strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="SimpleCircuitBreakerStrategyOptions{TResult}"/> for more details about the advanced circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static TBuilder AddSimpleCircuitBreaker<TBuilder, TResult>(this TBuilder builder, SimpleCircuitBreakerStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The circuit breaker strategy options are invalid.");

        builder.AddStrategy(context => CreateStrategy<TResult>(context, options, new ConsecutiveFailuresCircuitBehavior(options.FailureThreshold)), options);
        return builder;
    }

    internal static CircuitBreakerResilienceStrategy CreateStrategy(ResilienceStrategyBuilderContext context, CircuitBreakerStrategyOptions<object> options, CircuitBehavior behavior)
    {
        var controller = new CircuitStateController(
            options.BreakDuration,
            context.CreateInvoker(options.OnOpened),
            context.CreateInvoker(options.OnClosed),
            options.OnHalfOpened,
            behavior,
            context.TimeProvider,
            context.Telemetry);

        return new CircuitBreakerResilienceStrategy(
            context.CreateInvoker(options.ShouldHandle)!,
            controller,
            options.StateProvider,
            options.ManualControl);
    }

    internal static CircuitBreakerResilienceStrategy CreateStrategy<TResult>(ResilienceStrategyBuilderContext context, CircuitBreakerStrategyOptions<TResult> options, CircuitBehavior behavior)
    {
        var controller = new CircuitStateController(
            options.BreakDuration,
            context.CreateInvoker(options.OnOpened),
            context.CreateInvoker(options.OnClosed),
            options.OnHalfOpened,
            behavior,
            context.TimeProvider,
            context.Telemetry);

        return new CircuitBreakerResilienceStrategy(
            context.CreateInvoker(options.ShouldHandle)!,
            controller,
            options.StateProvider,
            options.ManualControl);
    }
}

