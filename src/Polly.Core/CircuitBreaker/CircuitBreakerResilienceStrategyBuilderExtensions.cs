using Polly.CircuitBreaker;
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
    /// See <see cref="CircuitBreakerStrategyOptions{TResult}"/> for more details about the advanced circuit breaker strategy.
    /// </remarks>
    public static ResilienceStrategyBuilder AddCircuitBreaker<TResult>(this ResilienceStrategyBuilder builder, CircuitBreakerStrategyOptions<TResult> options)
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
    /// See <see cref="CircuitBreakerStrategyOptions"/> for more details about the advanced circuit breaker strategy.
    /// </remarks>
    public static ResilienceStrategyBuilder AddCircuitBreaker(this ResilienceStrategyBuilder builder, CircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The circuit breaker strategy options are invalid.");

        return builder.AddCircuitBreakerCore(options);
    }

    private static ResilienceStrategyBuilder AddCircuitBreakerCore(this ResilienceStrategyBuilder builder, BaseCircuitBreakerStrategyOptions options)
    {
        return builder.AddStrategy(context => new CircuitBreakerResilienceStrategy(context.TimeProvider, context.Telemetry));
    }
}

