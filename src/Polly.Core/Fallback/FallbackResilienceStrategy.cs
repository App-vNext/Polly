using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Fallback;

internal sealed class FallbackResilienceStrategy : ResilienceStrategy
{
    private readonly FallbackHandler.Handler? _handler;
    private readonly OutcomeEvent<OnFallbackArguments>.Handler? _onFallback;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategy(FallbackStrategyOptions options, ResilienceStrategyTelemetry telemetry)
    {
        _handler = options.Handler.CreateHandler();
        _onFallback = options.OnFallback.CreateHandler();
        _telemetry = telemetry;
    }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        if (_handler == null)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        Outcome<TResult> outcome;
        var args = new HandleFallbackArguments(context);
        FallbackAction<TResult>? action;

        try
        {
            var result = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            outcome = new Outcome<TResult>(result);
            action = await _handler.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);

            if (action == null)
            {
                return result;
            }
        }
        catch (Exception e)
        {
            outcome = new Outcome<TResult>(e);
            action = await _handler.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);

            if (action == null)
            {
                throw;
            }
        }

        _telemetry.Report(FallbackConstants.OnFallback, outcome, args);

        if (_onFallback != null)
        {
            await _onFallback.HandleAsync(outcome, new OnFallbackArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return await action(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);
    }
}
