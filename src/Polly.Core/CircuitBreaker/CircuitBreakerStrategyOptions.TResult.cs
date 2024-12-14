using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly.CircuitBreaker;

/// <summary>
/// The options for circuit breaker resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
/// <remarks>
/// The circuit will break if, within any time-slice of duration <see cref="SamplingDuration"/>,
/// the proportion of actions resulting in a handled exception exceeds <see cref="FailureRatio"/>,
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
public class CircuitBreakerStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerStrategyOptions{TResult}"/> class.
    /// </summary>
    public CircuitBreakerStrategyOptions() => Name = CircuitBreakerConstants.DefaultName;

    /// <summary>
    /// Gets or sets the failure-to-success ratio at which the circuit will break.
    /// </summary>
    /// <remarks>
    /// A number between zero and one (inclusive) e.g. 0.5 represents breaking if 50% or more of actions result in a handled failure.
    /// </remarks>
    /// <value>A ratio number higher than 0, up to 1. The default value is 0.1 (i.e. 10%).</value>
    [Range(0, 1.0)]
    public double FailureRatio { get; set; } = CircuitBreakerConstants.DefaultFailureRatio;

    /// <summary>
    /// Gets or sets the minimum throughput: this many actions or more must pass through the circuit in the time-slice,
    /// for statistics to be considered significant and the circuit-breaker to come into action.
    /// </summary>
    /// <value>
    /// The default value is 100. The value must be 2 or greater.
    /// </value>
    [Range(CircuitBreakerConstants.MinimumValidThroughput, int.MaxValue)]
    public int MinimumThroughput { get; set; } = CircuitBreakerConstants.DefaultMinimumThroughput;

    /// <summary>
    /// Gets or sets the duration of the sampling over which failure ratios are assessed.
    /// </summary>
    /// <value>
    /// The default value is 30 seconds. Value must be greater than 0.5 seconds.
    /// </value>
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Addressed with DynamicDependency on ValidationHelper.Validate method")]
    public TimeSpan SamplingDuration { get; set; } = CircuitBreakerConstants.DefaultSamplingDuration;

    /// <summary>
    /// Gets or sets the duration of break the circuit will stay open before resetting.
    /// </summary>
    /// <value>
    /// The default value is 5 seconds. Value must be greater than 0.5 seconds.
    /// </value>
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Addressed with DynamicDependency on ValidationHelper.Validate method")]
    public TimeSpan BreakDuration { get; set; } = CircuitBreakerConstants.DefaultBreakDuration;

    /// <summary>
    /// Gets or sets an optional delegate to use to dynamically generate the break duration.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<BreakDurationGeneratorArguments<TResult>, ValueTask<TimeSpan>>? BreakDurationGenerator { get; set; }

    /// <summary>
    /// Gets or sets a predicate that determines whether the outcome should be handled by the circuit breaker.
    /// </summary>
    /// <value>
    /// The default value is a predicate that handles circuit breaker on any exception except <see cref="OperationCanceledException"/>.
    /// This property is required.
    /// </value>
    [Required]
    public Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<CircuitBreakerPredicateArguments<TResult>, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets the event that is raised when the circuit resets to a <see cref="CircuitState.Closed"/> state.
    /// </summary>
    /// <remarks>
    /// The callbacks registered to this event are invoked with eventual consistency. There is no guarantee that the circuit breaker
    /// doesn't change the state before the callbacks finish. If you need to know the up-to-date state of the circuit breaker use
    /// the <see cref="CircuitBreakerStateProvider.CircuitState"/> property.
    /// <para>
    /// Note that these events might be executed asynchronously at a later time when the circuit state is no longer the same as at the point of invocation of the event.
    /// However, the invocation order of the <see cref="OnOpened"/>, <see cref="OnClosed"/>, and <see cref="OnHalfOpened"/> events is always
    /// maintained to ensure the correct sequence of state transitions.
    /// </para>
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public Func<OnCircuitClosedArguments<TResult>, ValueTask>? OnClosed { get; set; }

    /// <summary>
    /// Gets or sets the event that is raised when the circuit transitions to an <see cref="CircuitState.Open"/> state.
    /// </summary>
    /// <remarks>
    /// The callbacks registered to this event are invoked with eventual consistency. There is no guarantee that the circuit breaker
    /// doesn't change the state before the callbacks finish. If you need to know the up-to-date state of the circuit breaker use
    /// the <see cref="CircuitBreakerStateProvider.CircuitState"/> property.
    /// <para>
    /// Note that these events might be executed asynchronously at a later time when the circuit state is no longer the same as at the point of invocation of the event.
    /// However, the invocation order of the <see cref="OnOpened"/>, <see cref="OnClosed"/>, and <see cref="OnHalfOpened"/> events is always
    /// maintained to ensure the correct sequence of state transitions.
    /// </para>
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public Func<OnCircuitOpenedArguments<TResult>, ValueTask>? OnOpened { get; set; }

    /// <summary>
    /// Gets or sets the event that is raised when when the circuit transitions to an <see cref="CircuitState.HalfOpen"/> state.
    /// </summary>
    /// <remarks>
    /// The callbacks registered to this event are invoked with eventual consistency. There is no guarantee that the circuit breaker
    /// doesn't change the state before the callbacks finish. If you need to know the up-to-date state of the circuit breaker use
    /// the <see cref="CircuitBreakerStateProvider.CircuitState"/> property.
    /// <para>
    /// Note that these events might be executed asynchronously at a later time when the circuit state is no longer the same as at the point of invocation of the event.
    /// However, the invocation order of the <see cref="OnOpened"/>, <see cref="OnClosed"/>, and <see cref="OnHalfOpened"/> events is always
    /// maintained to ensure the correct sequence of state transitions.
    /// </para>
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public Func<OnCircuitHalfOpenedArguments, ValueTask>? OnHalfOpened { get; set; }

    /// <summary>
    /// Gets or sets the manual control for the circuit breaker.
    /// </summary>
    /// <value>The default value is <see langword="null"/>.</value>
    public CircuitBreakerManualControl? ManualControl { get; set; }

    /// <summary>
    /// Gets or sets the state provider for the circuit breaker.
    /// </summary>
    /// <value>The default value is <see langword="null"/>.</value>
    public CircuitBreakerStateProvider? StateProvider { get; set; }
}

