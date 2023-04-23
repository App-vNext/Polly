using System;

namespace Polly.CircuitBreaker;

/// <summary>
/// Allows manual control of the circuit-breaker.
/// </summary>
public sealed class CircuitBreakerManualControl : IDisposable
{
    private Action? _onDispose;
    private Func<ResilienceContext, Task>? _onIsolate;
    private Func<ResilienceContext, Task>? _onReset;

    internal void Initialize(Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset, Action onDispose)
    {
        if (_onIsolate != null)
        {
            throw new InvalidOperationException($"This instance of '{nameof(CircuitBreakerManualControl)}' is already initialized and cannot be used in a different circuit-breaker strategy.");
        }

        _onDispose = onDispose;
        _onIsolate = onIsolate;
        _onReset = onReset;
    }

    /// <summary>
    /// Gets a value indicating whether the manual control is initialized.
    /// </summary>
    /// <remarks>
    /// The initialization happens when the circuit-breaker strategy is attached to this class.
    /// This happens when the final strategy is created by the <see cref="ResilienceStrategyBuilder.Build"/> call.
    /// </remarks>
    public bool IsInitialized => _onIsolate != null;

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="CloseAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="context">The resilience context.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown if manual control is not initialized.</exception>
    public Task IsolateAsync(ResilienceContext context)
    {
        Guard.NotNull(context);

        if (_onIsolate == null)
        {
            throw new InvalidOperationException("The circuit-breaker manual control is not initialized");
        }

        context.Initialize<VoidResult>(isSynchronous: false);
        return _onIsolate(context);
    }

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="CloseAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown if manual control is not initialized.</exception>
    public async Task IsolateAsync(CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken;

        try
        {
            await IsolateAsync(context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    /// <param name="context">The resilience context.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown if manual control is not initialized.</exception>
    public Task CloseAsync(ResilienceContext context)
    {
        Guard.NotNull(context);

        if (_onReset == null)
        {
            throw new InvalidOperationException("The circuit-breaker manual control is not initialized");
        }

        context.Initialize<VoidResult>(isSynchronous: false);
        return _onReset(context);
    }

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown if manual control is not initialized.</exception>
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken;

        try
        {
            await CloseAsync(context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _onDispose?.Invoke();
}
