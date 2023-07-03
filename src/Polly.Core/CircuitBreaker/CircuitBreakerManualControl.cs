using System.Collections.Generic;

namespace Polly.CircuitBreaker;

/// <summary>
/// Allows manual control of the circuit-breaker.
/// </summary>
/// <remarks>
/// The instance of this class can be reused across multiple circuit breakers.
/// </remarks>
public sealed class CircuitBreakerManualControl : IDisposable
{
    private readonly HashSet<Action> _onDispose = new();
    private readonly HashSet<Func<ResilienceContext, Task>> _onIsolate = new();
    private readonly HashSet<Func<ResilienceContext, Task>> _onReset = new();
    private bool _isolated;

    internal void Initialize(Func<ResilienceContext, Task> onIsolate, Func<ResilienceContext, Task> onReset, Action onDispose)
    {
        _onDispose.Add(onDispose);
        _onIsolate.Add(onIsolate);
        _onReset.Add(onReset);

        if (_isolated)
        {
            var context = ResilienceContext.Get().Initialize<VoidResult>(isSynchronous: true);

            // if the control indicates that circuit breaker should be isolated, we isolate it right away
            IsolateAsync(context).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="CloseAsync(CancellationToken)"/> is made.
    /// </summary>
    /// <param name="context">The resilience context.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after this object is disposed.</exception>
    internal async Task IsolateAsync(ResilienceContext context)
    {
        Guard.NotNull(context);

        _isolated = true;

        foreach (var action in _onIsolate)
        {
            await action(context).ConfigureAwait(context.ContinueOnCapturedContext);
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
        var context = ResilienceContext.Get(cancellationToken).Initialize<VoidResult>(isSynchronous: false);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after this object is disposed.</exception>
    internal async Task CloseAsync(ResilienceContext context)
    {
        Guard.NotNull(context);

        _isolated = false;

        context.Initialize<VoidResult>(isSynchronous: false);

        foreach (var action in _onReset)
        {
            await action(context).ConfigureAwait(context.ContinueOnCapturedContext);
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
        var context = ResilienceContext.Get(cancellationToken);

        try
        {
            await CloseAsync(context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    /// <summary>
    /// Disposes the current class.
    /// </summary>
    public void Dispose()
    {
        foreach (var action in _onDispose)
        {
            action();
        }

        _onDispose.Clear();
        _onIsolate.Clear();
        _onReset.Clear();
    }
}
