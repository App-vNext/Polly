namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy<T> : ResilienceStrategy<T>, IDisposable
{
    private readonly Func<CircuitBreakerPredicateArguments<T>, ValueTask<bool>> _handler;
    private readonly CircuitStateController<T> _controller;
    private readonly IDisposable? _manualControlRegistration;

    public CircuitBreakerResilienceStrategy(
        Func<CircuitBreakerPredicateArguments<T>, ValueTask<bool>> handler,
        CircuitStateController<T> controller,
        CircuitBreakerStateProvider? stateProvider,
        CircuitBreakerManualControl? manualControl)
    {
        _handler = handler;
        _controller = controller;

        stateProvider?.Initialize(() => _controller.CircuitState);
        _manualControlRegistration = manualControl?.Initialize(
            _controller.IsolateCircuitAsync,
            _controller.CloseCircuitAsync);
    }

    public void Dispose()
    {
        _manualControlRegistration?.Dispose();
        _controller.Dispose();
    }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        if (await _controller.OnActionPreExecuteAsync(context).ConfigureAwait(context.ContinueOnCapturedContext) is Outcome<T> outcome)
        {
            return outcome;
        }

        try
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
#pragma warning disable CA1031
        catch (Exception ex)
        {
            outcome = new(ex);
        }
#pragma warning restore CA1031

        var args = new CircuitBreakerPredicateArguments<T>(context, outcome);
        if (await _handler(args).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            await _controller.OnHandledOutcomeAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        else
        {
            await _controller.OnUnhandledOutcomeAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }
}

