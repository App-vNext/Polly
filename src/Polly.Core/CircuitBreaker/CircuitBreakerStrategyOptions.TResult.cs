using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// The base options for circuit breaker resilience strategy.
/// </summary>
/// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
/// <remarks>
/// The circuit will stay broken for the <see cref="BreakDuration"/>. Any attempt to execute the resilience strategy
/// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
/// that broke the circuit.
/// <para>
/// If the first action after the break duration period results in a handled exception or result, the circuit will break
/// again for another <see cref="BreakDuration"/>; if no exception or handled result is encountered, the circuit will reset.
/// </para>
/// </remarks>
public abstract class CircuitBreakerStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>CircuitBreaker</c> value.</remarks>
    public sealed override string StrategyType => CircuitBreakerConstants.StrategyType;

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
    /// <remarks>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, CircuitBreakerPredicateArguments, ValueTask<bool>>? ShouldHandle { get; set; }

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
    /// <para>
    /// Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public Func<Outcome<TResult>, OnCircuitClosedArguments, ValueTask>? OnClosed { get; set; }

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
    /// <para>
    /// Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public Func<Outcome<TResult>, OnCircuitOpenedArguments, ValueTask>? OnOpened { get; set; }

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
    /// <para>
    /// Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public Func<OnCircuitHalfOpenedArguments, ValueTask>? OnHalfOpened { get; set; }

    /// <summary>
    /// Gets or sets the manual control for the circuit breaker.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public CircuitBreakerManualControl? ManualControl { get; set; }

    /// <summary>
    /// Gets or sets the state provider for the circuit breaker.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public CircuitBreakerStateProvider? StateProvider { get; set; }

    internal void UpdateNonGenericOptions(CircuitBreakerStrategyOptions options)
    {
        options.BreakDuration = BreakDuration;
        options.StrategyName = StrategyName;

        if (ShouldHandle is var shouldHandle)
        {
            options.ShouldHandle = (outcome, args) =>
            {
                if (args.Context.ResultType != typeof(TResult))
                {
                    return new ValueTask<bool>(false);
                }

                return shouldHandle!(outcome.AsOutcome<TResult>(), args);
            };
        }

        if (OnClosed is var onClosed)
        {
            options.OnClosed = (outcome, args) =>
            {
                if (args.Context.ResultType != typeof(TResult))
                {
                    return default;
                }

                return onClosed!(outcome.AsOutcome<TResult>(), args);
            };
        }

        if (OnOpened is var onOpened)
        {
            options.OnOpened = (outcome, args) =>
            {
                if (args.Context.ResultType != typeof(TResult))
                {
                    return default;
                }

                return onOpened!(outcome.AsOutcome<TResult>(), args);
            };
        }

        options.OnHalfOpened = OnHalfOpened;
        options.ManualControl = ManualControl;
        options.StateProvider = StateProvider;
    }
}

