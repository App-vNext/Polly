namespace Polly.CircuitBreaker;

/// <summary>
/// Allows manual control of the circuit-breaker.
/// </summary>
/// <remarks>
/// The instance of this class can be reused across multiple circuit breakers.
/// </remarks>
public sealed class CircuitBreakerManualControl
{
    private readonly object _lock = new();
    private readonly HashSet<Func<ResilienceContext, Task>> _onIsolate = [];
    private readonly HashSet<Func<ResilienceContext, Task>> _onReset = [];
    private bool _isolated;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerManualControl"/> class.
    /// </summary>
    public CircuitBreakerManualControl()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerManualControl"/> class.
    /// </summary>
    /// <param name="isIsolated">Determines whether the circuit breaker is isolated immediately after construction.</param>
    public CircuitBreakerManualControl(bool isIsolated) => _isolated = isIsolated;

    internal bool IsEmpty => _onIsolate.Count == 0;

    internal IDisposable Initialize(Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset)
    {
        bool isolated;
        lock (_lock)
        {
            _onIsolate.Add(onIsolate);
            _onReset.Add(onReset);
            isolated = _isolated;
        }

        // if the control indicates that circuit breaker should be isolated, we isolate it right away
        if (isolated)
        {
            var context = ResilienceContextPool.Shared.Get().Initialize<VoidResult>(isSynchronous: true);
            onIsolate(context).GetAwaiter().GetResult();
        }

        return new RegistrationDisposable(this, onIsolate, onReset);
    }

    private void Remove(Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset)
    {
        lock (_lock)
        {
            _onIsolate.Remove(onIsolate);
            _onReset.Remove(onReset);
        }
    }

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="CloseAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after this object is disposed.</exception>
    public async Task IsolateAsync(CancellationToken cancellationToken = default)
    {
        Func<ResilienceContext, Task>[] callbacks;
        lock (_lock)
        {
            callbacks = [.. _onIsolate];
            _isolated = true;
        }

        var context = ResilienceContextPool.Shared.Get(cancellationToken).Initialize<VoidResult>(isSynchronous: false);

        try
        {
            foreach (var action in callbacks)
            {
                await action(context).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after this object is disposed.</exception>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        Func<ResilienceContext, Task>[] callbacks;
        lock (_lock)
        {
            callbacks = [.. _onReset];
            _isolated = false;
        }

        var context = ResilienceContextPool.Shared.Get(cancellationToken).Initialize<VoidResult>(isSynchronous: false);

        try
        {
            foreach (var action in callbacks)
            {
                await action(context).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private sealed class RegistrationDisposable(CircuitBreakerManualControl owner, Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset) : IDisposable
    {
        private readonly CircuitBreakerManualControl _owner = owner;
        private readonly Func<ResilienceContext, Task> _onIsolate = onIsolate;
        private readonly Func<ResilienceContext, Task> _onReset = onReset;

        public void Dispose() => _owner.Remove(_onIsolate, _onReset);
    }
}
