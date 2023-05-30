using System.ComponentModel.DataAnnotations;

namespace Polly.CircuitBreaker;

/// <summary>
/// The options for advanced circuit breaker resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
/// <remarks>
/// The circuit will break if, within any time-slice of duration <see cref="SamplingDuration"/>,
/// the proportion of actions resulting in a handled exception exceeds <see cref="FailureThreshold"/>,
/// provided also that the number of actions through the circuit in the time-slice is at least <see cref="MinimumThroughput"/>.
/// <para>
/// The circuit will stay broken for the <see cref="CircuitBreakerStrategyOptions{TResult}.BreakDuration"/>.
/// Any attempt to execute this while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception
/// that broke the circuit.
/// </para>
/// <para>
/// If the first action after the break duration period results in a handled exception, the circuit will break
/// again for another <see cref="CircuitBreakerStrategyOptions{TResult}.BreakDuration"/>; if no exception is thrown, the circuit will reset.
/// </para>
/// </remarks>
public class AdvancedCircuitBreakerStrategyOptions<TResult> : CircuitBreakerStrategyOptions<TResult>
{
    /// <summary>
    /// Gets or sets the failure threshold at which the circuit will break.
    /// </summary>
    /// <remarks>
    /// A number between zero and one (inclusive) e.g. 0.5 represents breaking if 50% or more of actions result in a handled failure.
    /// <para>
    /// A ratio number higher than 0, up to 1.
    /// Defaults to 0.1 (i.e. 10%).
    /// </para>
    /// </remarks>
    [Range(0, 1.0)]
    public double FailureThreshold { get; set; } = CircuitBreakerConstants.DefaultAdvancedFailureThreshold;

    /// <summary>
    /// Gets or sets the minimum throughput: this many actions or more must pass through the circuit in the time-slice,
    /// for statistics to be considered significant and the circuit-breaker to come into action.
    /// </summary>
    /// <remarks>
    /// Value must be 2 or greater.
    /// Defaults to 100.
    /// </remarks>
    [Range(CircuitBreakerConstants.MinimumValidThroughput, int.MaxValue)]
    public int MinimumThroughput { get; set; } = CircuitBreakerConstants.DefaultMinimumThroughput;

    /// <summary>
    /// Gets or sets the duration of the sampling over which failure ratios are assessed.
    /// </summary>
    /// <remarks>
    /// Value must be greater than 0.5 seconds. Defaults to 30 seconds.
    /// </remarks>
    [TimeSpan("00:00:00.500")]
    public TimeSpan SamplingDuration { get; set; } = CircuitBreakerConstants.DefaultSamplingDuration;
}
