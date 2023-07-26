namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy<T> : OutcomeResilienceStrategy<T>
{
    private readonly Func<OutcomeArguments<T, CircuitBreakerPredicateArguments>, ValueTask<bool>> _handler;
    private readonly CircuitStateController<T> _controller;

    public CircuitBreakerResilienceStrategy(
        Func<OutcomeArguments<T, CircuitBreakerPredicateArguments>, ValueTask<bool>> handler,
        CircuitStateController<T> controller,
        CircuitBreakerStateProvider? stateProvider,
        CircuitBreakerManualControl? manualControl)
    {
        _handler = handler;
        _controller = controller;

        stateProvider?.Initialize(() => _controller.CircuitState, () => _controller.LastHandledOutcome);
        manualControl?.Initialize(
            async c => await _controller.IsolateCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            async c => await _controller.CloseCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            _controller.Dispose);
    }

    protected override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        if (await _controller.OnActionPreExecuteAsync(context).ConfigureAwait(context.ContinueOnCapturedContext) is Outcome<T> outcome)
        {
            return outcome;
        }

        outcome = await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);

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

    protected override Outcome<T> ExecuteCoreSync<TState>(Func<ResilienceContext, TState, Outcome<T>> callback, ResilienceContext context, TState state)
    {
        return ExecuteCore(static (c, s) =>
        {
            return new ValueTask<Outcome<T>>(s.callback(c, s.state));
        },
        context,
        (callback, state)).GetAwaiter().GetResult();
    }
}

