using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// The base options for circuit breaker resilience strategy.
/// <para>The circuit will stay broken for the <see cref="BreakDuration"/>. Any attempt to execute the resilience strategy
/// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
/// that broke the circuit.
/// </para>
/// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
/// again for another <see cref="BreakDuration"/>; if no exception or handled result is encountered, the circuit will reset.
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
public abstract class BaseCircuitBreakerStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseCircuitBreakerStrategyOptions{TResult}"/> class.
    /// </summary>
    protected BaseCircuitBreakerStrategyOptions() => StrategyType = CircuitBreakerConstants.StrategyType;

    /// <summary>
    /// Gets or sets the duration of break the circuit will stay open before resetting.
    /// </summary>
    /// <remarks>
    /// Value must be greater than 0.5 seconds.
    /// Defaults to 5 seconds.
    /// </remarks>
    [TimeSpan("00:00:00.500")]
    public TimeSpan BreakDuration { get; set; } = CircuitBreakerConstants.DefaultBreakDuration;

    /// <summary>
    /// Gets or sets the predicates for the circuit breaker.
    /// </summary>
    [Required]
    public OutcomePredicate<CircuitBreakerPredicateArguments, TResult> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the event that is raised when the circuit resets to a <see cref="CircuitState.Closed"/> state.
    /// </summary>
    [Required]
    public OutcomeEvent<OnCircuitResetArguments, TResult> OnReset { get; set; } = new();

    /// <summary>
    /// Gets or sets the event that is raised when the circuit transitions to an <see cref="CircuitState.Open"/> state.
    /// </summary>
    [Required]
    public OutcomeEvent<OnCircuitBreakArguments, TResult> OnBreak { get; set; } = new();

    /// <summary>
    /// Gets or sets the event that is raised when when the circuit transitions to an <see cref="CircuitState.HalfOpen"/> state.
    /// </summary>
    [Required]
    public NoOutcomeEvent<OnCircuitHalfOpenArguments> OnHalfOpen { get; set; } = new();

    /// <summary>
    /// Gets or sets the manual control for the circuit breaker.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    public CircuitBreakerManualControl? ManualControl { get; set; }

    /// <summary>
    /// Gets or sets the state provider for the circuit breaker.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    public CircuitBreakerStateProvider? StateProvider { get; set; }

    internal void UpdateNonGenericOptions(BaseCircuitBreakerStrategyOptions options)
    {
        options.BreakDuration = BreakDuration;
        options.StrategyName = StrategyName;
        options.StrategyType = StrategyType;
        options.ShouldHandle = new OutcomePredicate<CircuitBreakerPredicateArguments>().SetPredicates(ShouldHandle);
        options.OnReset = new OutcomeEvent<OnCircuitResetArguments>().SetCallbacks(OnReset);
        options.OnBreak = new OutcomeEvent<OnCircuitBreakArguments>().SetCallbacks(OnBreak);
        options.OnHalfOpen = OnHalfOpen;
        options.ManualControl = ManualControl;
        options.StateProvider = StateProvider;
    }
}
