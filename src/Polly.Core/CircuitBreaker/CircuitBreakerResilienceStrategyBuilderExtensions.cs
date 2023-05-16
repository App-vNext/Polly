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
    public static ResilienceStrategyBuilder AddAdvancedCircuitBreaker<TResult>(this ResilienceStrategyBuilder builder, AdvancedCircuitBreakerStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The advanced circuit breaker strategy options are invalid.");

        return builder.AddCircuitBreakerCore(options.AsNonGenericOptions());
    }

    /// <summary>
    /// Add advanced circuit breaker strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="AdvancedCircuitBreakerStrategyOptions"/> for more details about the advanced circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    public static ResilienceStrategyBuilder AddAdvancedCircuitBreaker(this ResilienceStrategyBuilder builder, AdvancedCircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The advanced circuit breaker strategy options are invalid.");

        return builder.AddCircuitBreakerCore(options);
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
    public static ResilienceStrategyBuilder AddSimpleCircuitBreaker<TResult>(this ResilienceStrategyBuilder builder, SimpleCircuitBreakerStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The circuit breaker strategy options are invalid.");

        return builder.AddCircuitBreakerCore(options.AsNonGenericOptions());
    }

    /// <summary>
    /// Add simple circuit breaker strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="SimpleCircuitBreakerStrategyOptions"/> for more details about the advanced circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    public static ResilienceStrategyBuilder AddSimpleCircuitBreaker(this ResilienceStrategyBuilder builder, SimpleCircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The circuit breaker strategy options are invalid.");

        return builder.AddCircuitBreakerCore(options);
    }

    internal static ResilienceStrategyBuilder AddCircuitBreakerCore(this ResilienceStrategyBuilder builder, CircuitBreakerStrategyOptions options)
    {
        return builder.AddStrategy(context =>
        {
            CircuitBehavior behavior = options switch
            {
                AdvancedCircuitBreakerStrategyOptions o => new AdvancedCircuitBehavior(o, HealthMetrics.Create(o, context.TimeProvider)),
                SimpleCircuitBreakerStrategyOptions o => new ConsecutiveFailuresCircuitBehavior(o),
                _ => throw new NotSupportedException()
            };

            var controller = new CircuitStateController(options, behavior, context.TimeProvider, context.Telemetry);

            return new CircuitBreakerResilienceStrategy(options, controller);
        });
    }
}

