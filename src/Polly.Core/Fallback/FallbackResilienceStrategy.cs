using Polly.Telemetry;

namespace Polly.Fallback;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class FallbackResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly FallbackHandler<T> _handler;
    private readonly Func<OutcomeArguments<T, OnFallbackArguments>, ValueTask>? _onFallback;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategy(FallbackHandler<T> handler, Func<OutcomeArguments<T, OnFallbackArguments>, ValueTask>? onFallback, ResilienceStrategyTelemetry telemetry)
    {
        _handler = handler;
        _onFallback = onFallback;
        _telemetry = telemetry;
    }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        var outcome = await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var handleFallbackArgs = new OutcomeArguments<T, FallbackPredicateArguments>(context, outcome, default);
        if (!await _handler.ShouldHandle(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            return outcome;
        }

        var onFallbackArgs = new OutcomeArguments<T, OnFallbackArguments>(context, outcome, default);

        _telemetry.Report(new(ResilienceEventSeverity.Warning, FallbackConstants.OnFallback), onFallbackArgs);

        if (_onFallback is not null)
        {
            await _onFallback(onFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        try
        {
            return await _handler.GetFallbackOutcomeAsync(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            return Outcome.FromException<T>(e);
        }
    }
}
