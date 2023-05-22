using Polly.Strategy;

namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy : ResilienceStrategy
{
    private readonly CircuitStateController _controller;
    private readonly OutcomePredicate<CircuitBreakerPredicateArguments>.Handler? _handler;

    public CircuitBreakerResilienceStrategy(CircuitBreakerStrategyOptions options, CircuitStateController controller)
    {
        _controller = controller;
        _handler = options.ShouldHandle.CreateHandler();

        options.StateProvider?.Initialize(() => _controller.CircuitState, () => _controller.LastHandledOutcome);
        options.ManualControl?.Initialize(
            async c => await _controller.IsolateCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            async c => await _controller.CloseCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            _controller.Dispose);
    }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (_handler == null)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        if (await _controller.OnActionPreExecuteAsync<TResult>(context).ConfigureAwait(context.ContinueOnCapturedContext) is Outcome<TResult> outcome)
        {
            return outcome;
        }

        outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        var args = new CircuitBreakerPredicateArguments(context);
        if (await _handler!.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext))
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

