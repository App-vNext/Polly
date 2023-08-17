namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy<T> : ResilienceStrategy<T>, IDisposable
{
    private readonly Func<OutcomeArguments<T, CircuitBreakerPredicateArguments>, ValueTask<bool>> _handler;
    private readonly CircuitStateController<T> _controller;
    private readonly IDisposable? _manualControlRegistration;

    public CircuitBreakerResilienceStrategy(
        Func<OutcomeArguments<T, CircuitBreakerPredicateArguments>, ValueTask<bool>> handler,
        CircuitStateController<T> controller,
        CircuitBreakerStateProvider? stateProvider,
        CircuitBreakerManualControl? manualControl)
    {
        _handler = handler;
        _controller = controller;

        stateProvider?.Initialize(() => _controller.CircuitState, () => _controller.LastHandledOutcome);
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

        var args = new OutcomeArguments<T, CircuitBreakerPredicateArguments>(context, outcome, default);
        if (await _handler(args).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            await _controller.OnActionFailureAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        else if (outcome.Exception is null)
        {
            await _controller.OnActionSuccessAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }
}

