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
            async c => await _controller.IsolateCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            async c => await _controller.CloseCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext));
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

        outcome = await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);

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

