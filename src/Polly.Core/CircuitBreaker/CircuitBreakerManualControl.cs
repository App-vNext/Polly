using System;

namespace Polly.CircuitBreaker;

/// <summary>
/// Allows manual control of the circuit-breaker.
/// </summary>
public sealed class CircuitBreakerManualControl
{
    private Func<ResilienceContext, Task>? _onIsolate;
    private Func<ResilienceContext, Task>? _onReset;

    internal void Initialize(Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset)
    {
        if (_onIsolate != null)
        {
            throw new InvalidOperationException($"This instance of '{nameof(CircuitBreakerManualControl)}' is already initialized and cannot be used in a different circuit-breaker strategy.");
        }

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
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="ResetAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="context">The resilience context.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <remarks>
    /// This operation throws an <see cref="InvalidOperationException"/> if the manual control is not initialized.
    /// </remarks>
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
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="ResetAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <remarks>
    /// This operation throws an <see cref="InvalidOperationException"/> if the manual control is not initialized.
    /// </remarks>
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
    /// <remarks>
    /// This operation throws an <see cref="InvalidOperationException"/> if the manual control is not initialized.
    /// </remarks>
    public Task ResetAsync(ResilienceContext context)
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
    /// <remarks>
    /// This operation throws an <see cref="InvalidOperationException"/> if the manual control is not initialized.
    /// </remarks>
    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken;

        try
        {
            await ResetAsync(context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }
}
