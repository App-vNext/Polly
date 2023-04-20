using Polly.Strategy;

namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy : ResilienceStrategy
{
    private readonly CircuitStateController _controller;
    private readonly OutcomePredicate<CircuitBreakerPredicateArguments>.Handler? _handler;

    public CircuitBreakerResilienceStrategy(BaseCircuitBreakerStrategyOptions options, CircuitStateController controller)
    {
        _controller = controller;
        _handler = options.ShouldHandle.CreateHandler();

        options.StateProvider?.Initialize(() => _controller.CircuitState, () => _controller.LastHandledOutcome);
        options.ManualControl?.Initialize(
            async c => await _controller.IsolateCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            async c => await _controller.CloseCircuitAsync(c).ConfigureAwait(c.ContinueOnCapturedContext),
            _controller.Dispose);
    }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        if (_handler == null)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        await _controller.OnActionPreExecuteAsync(context).ConfigureAwait(context.ContinueOnCapturedContext);

        try
        {
            var result = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            var outcome = new Outcome<TResult>(result);

            if (await _handler.ShouldHandleAsync(outcome, new CircuitBreakerPredicateArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                await _controller.OnActionFailureAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
            }
            else
            {
                await _controller.OnActionSuccessAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            return result;
        }
        catch (Exception e)
        {
            var outcome = new Outcome<TResult>(e);

            if (await _handler.ShouldHandleAsync(outcome, new CircuitBreakerPredicateArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                await _controller.OnActionFailureAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            throw;
        }
    }
}

