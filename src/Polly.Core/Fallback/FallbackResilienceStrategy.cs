using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Fallback;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class FallbackResilienceStrategy : ResilienceStrategy
{
    private readonly FallbackHandler.Handler _handler;
    private readonly EventInvoker<OnFallbackArguments>? _onFallback;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategy(FallbackHandler.Handler handler, EventInvoker<OnFallbackArguments>? onFallback, ResilienceStrategyTelemetry telemetry)
    {
        _handler = handler;
        _onFallback = onFallback;
        _telemetry = telemetry;
    }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var args = new HandleFallbackArguments(context);
        var action = await _handler.ShouldHandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);

        if (action == null)
        {
            return outcome;
        }

        _telemetry.Report(FallbackConstants.OnFallback, outcome, args);

        if (_onFallback is not null)
        {
            await _onFallback.HandleAsync(outcome, new OnFallbackArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        try
        {
            return new Outcome<TResult>(await action(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext));
        }
        catch (Exception e)
        {
            return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
        }
    }
}
