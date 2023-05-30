using Polly.Strategy;

namespace Polly.CircuitBreaker;

/// <summary>
/// Allows retrieval of the circuit breaker state.
/// </summary>
public sealed class CircuitBreakerStateProvider
{
    private Func<CircuitState>? _circuitStateProvider;
    private Func<Outcome<object>?>? _lastHandledOutcomeProvider;

    internal void Initialize(Func<CircuitState> circuitStateProvider, Func<Outcome<object>?> lastHandledOutcomeProvider)
    {
        if (_circuitStateProvider != null)
        {
            throw new InvalidOperationException($"This instance of '{nameof(CircuitBreakerStateProvider)}' is already initialized and cannot be used in a different circuit-breaker strategy.");
        }

        _circuitStateProvider = circuitStateProvider;
        _lastHandledOutcomeProvider = lastHandledOutcomeProvider;
    }

    /// <summary>
    /// Gets a value indicating whether the state provider is initialized.
    /// </summary>
    /// <remarks>
    /// The initialization happens when the circuit-breaker strategy is attached to this class.
    /// This happens when the final strategy is created by the <see cref="ResilienceStrategyBuilder.Build"/> call.
    /// </remarks>
    internal bool IsInitialized => _circuitStateProvider != null;

    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    public CircuitState CircuitState => _circuitStateProvider?.Invoke() ?? CircuitState.Closed;

    /// <summary>
    /// Gets the last outcome handled by the circuit-breaker.
    /// <remarks>
    /// This will be null if no exceptions or results have been handled by the circuit-breaker since the circuit last closed.</remarks>
    /// </summary>
    public Outcome<object>? LastHandledOutcome => _lastHandledOutcomeProvider?.Invoke();
}
