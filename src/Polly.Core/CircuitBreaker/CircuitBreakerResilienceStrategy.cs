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

            await HandleResultAsync(context, result).ConfigureAwait(context.ContinueOnCapturedContext);

            return result;
        }
        catch (Exception e)
        {
            await HandleExceptionAsync<TResult>(context, e).ConfigureAwait(context.ContinueOnCapturedContext);

            throw;
        }
    }

    private async Task HandleResultAsync<TResult>(ResilienceContext context, TResult result)
    {
        var outcome = new Outcome<TResult>(result);
        var args = new CircuitBreakerPredicateArguments(context);
        if (await _handler!.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            await _controller.OnActionFailureAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        else
        {
            await _controller.OnActionSuccessAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }

    private async Task HandleExceptionAsync<TResult>(ResilienceContext context, Exception e)
    {
        var args = new CircuitBreakerPredicateArguments(context);
        var outcome = new Outcome<TResult>(e);

        if (await _handler!.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            await _controller.OnActionFailureAsync(outcome, context).ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }
}

