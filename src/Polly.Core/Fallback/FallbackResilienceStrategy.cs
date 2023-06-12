using System.Runtime.ExceptionServices;
using Polly.Telemetry;

namespace Polly.Fallback;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class FallbackResilienceStrategy<T> : ResilienceStrategy
{
    private readonly FallbackHandler<T> _handler;
    private readonly EventInvoker<OnFallbackArguments>? _onFallback;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategy(FallbackHandler<T> handler, EventInvoker<OnFallbackArguments>? onFallback, ResilienceStrategyTelemetry telemetry)
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
        if (!_handler.HandlesFallback<TResult>())
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var handleFallbackArgs = new OutcomeArguments<TResult, HandleFallbackArguments>(context, outcome, new HandleFallbackArguments());
        if (!await _handler.ShouldHandle.HandleAsync(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext))
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
            return await _handler.GetFallbackOutcomeAsync(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
        }
    }
}
