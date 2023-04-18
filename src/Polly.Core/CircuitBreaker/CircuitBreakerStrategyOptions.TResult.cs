using System.ComponentModel.DataAnnotations;

namespace Polly.CircuitBreaker;

/// <summary>
/// The options for circuit breaker resilience strategy.
/// <para>The circuit will break if <see cref="FailureThreshold"/>
/// exceptions or results that are handled by the resilience strategy are encountered consecutively. </para>
/// <para>The circuit will stay broken for the <see cref="BaseCircuitBreakerStrategyOptions.BreakDuration"/>. Any attempt to execute the resilience strategy
/// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
/// that broke the circuit.
/// </para>
/// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
/// again for another <see cref="BaseCircuitBreakerStrategyOptions.BreakDuration"/>; if no exception or handled result is encountered, the circuit will reset.
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
public class CircuitBreakerStrategyOptions<TResult> : BaseCircuitBreakerStrategyOptions<TResult>
{
    /// <summary>
    /// Gets or sets the number of the outcome failures handled by <see cref="BaseCircuitBreakerStrategyOptions.ShouldHandle"/> before opening the circuit.
    /// </summary>
    /// <remarks>
    /// Must be greater than 0. Defaults to 100.
    /// </remarks>
    [Range(1, int.MaxValue)]
    public int FailureThreshold { get; set; } = CircuitBreakerConstants.DefaultFailureThreshold;

    internal CircuitBreakerStrategyOptions AsNonGenericOptions()
    {
        var options = new CircuitBreakerStrategyOptions();
        UpdateNonGenericOptions(options);
        options.FailureThreshold = FailureThreshold;

        return options;
    }
}
