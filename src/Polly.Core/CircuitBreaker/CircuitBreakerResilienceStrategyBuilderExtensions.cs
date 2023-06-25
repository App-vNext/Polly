using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;

namespace Polly;

/// <summary>
/// Circuit breaker strategy extensions for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class CircuitBreakerResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Add advanced circuit breaker strategy to the builder.
    /// </summary>
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
    public static ResilienceStrategyBuilder AddAdvancedCircuitBreaker(this ResilienceStrategyBuilder builder, AdvancedCircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddAdvancedCircuitBreakerCore(options);
    }

    /// <summary>
    /// Add advanced circuit breaker strategy to the builder.
    /// </summary>
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
    public static ResilienceStrategyBuilder<TResult> AddAdvancedCircuitBreaker<TResult>(this ResilienceStrategyBuilder<TResult> builder, AdvancedCircuitBreakerStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddAdvancedCircuitBreakerCore(options);
    }

    /// <summary>
    /// Add simple circuit breaker strategy to the builder.
    /// </summary>
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
    public static ResilienceStrategyBuilder<TResult> AddSimpleCircuitBreaker<TResult>(this ResilienceStrategyBuilder<TResult> builder, SimpleCircuitBreakerStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddSimpleCircuitBreakerCore(options);
    }

    /// <summary>
    /// Add simple circuit breaker strategy to the builder.
    /// </summary>
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
    public static ResilienceStrategyBuilder AddSimpleCircuitBreaker(this ResilienceStrategyBuilder builder, SimpleCircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddSimpleCircuitBreakerCore(options);
    }

    private static TBuilder AddAdvancedCircuitBreakerCore<TBuilder, TResult>(this TBuilder builder, AdvancedCircuitBreakerStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(
            context =>
            {
                var behavior = new AdvancedCircuitBehavior(
                    options.FailureThreshold,
                    options.MinimumThroughput,
                    HealthMetrics.Create(options.SamplingDuration, context.TimeProvider));

                return CreateStrategy(context, options, behavior);
            },
            options);
    }

    private static TBuilder AddSimpleCircuitBreakerCore<TBuilder, TResult>(this TBuilder builder, SimpleCircuitBreakerStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(context => CreateStrategy(context, options, new ConsecutiveFailuresCircuitBehavior(options.FailureThreshold)), options);
    }

    internal static CircuitBreakerResilienceStrategy<TResult> CreateStrategy<TResult>(
        ResilienceStrategyBuilderContext context,
        CircuitBreakerStrategyOptions<TResult> options,
        CircuitBehavior behavior)
    {
        var controller = new CircuitStateController<TResult>(
            options.BreakDuration,
            options.OnOpened,
            options.OnClosed,
            options.OnHalfOpened,
            behavior,
            context.TimeProvider,
            context.Telemetry);

        return new CircuitBreakerResilienceStrategy<TResult>(
            options.ShouldHandle!,
            controller,
            options.StateProvider,
            options.ManualControl,
            context.IsGenericBuilder);
    }
}

