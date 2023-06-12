using System.Runtime.ExceptionServices;
using Polly.Telemetry;

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
        var outcome = await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var handleFallbackArgs = new OutcomeArguments<TResult, HandleFallbackArguments>(context, outcome, new HandleFallbackArguments());
        var action = await _handler.ShouldHandleAsync(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);

        if (action == null)
        {
            return outcome;
        }

        var onFallbackArgs = new OutcomeArguments<TResult, OnFallbackArguments>(context, outcome, new OnFallbackArguments());

        _telemetry.Report(FallbackConstants.OnFallback, onFallbackArgs);

        if (_onFallback is not null)
        {
            await _onFallback.HandleAsync(onFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        try
        {
            return new Outcome<TResult>(await action(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext));
        }
        catch (Exception e)
        {
            return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
        }
    }
}
