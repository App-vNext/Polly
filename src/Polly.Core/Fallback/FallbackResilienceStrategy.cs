using Polly.Telemetry;

namespace Polly.Fallback;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class FallbackResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly FallbackHandler<T> _handler;
    private readonly Func<OnFallbackArguments<T>, ValueTask>? _onFallback;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategy(FallbackHandler<T> handler, Func<OnFallbackArguments<T>, ValueTask>? onFallback, ResilienceStrategyTelemetry telemetry)
    {
        _handler = handler;
        _onFallback = onFallback;
        _telemetry = telemetry;
    }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        Outcome<T> outcome;
        try
        {
            outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (Exception ex)
        {
            outcome = new(ex);
        }

        var handleFallbackArgs = new FallbackPredicateArguments<T>(context, outcome);
        if (!await _handler.ShouldHandle(handleFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            return outcome;
        }

        var onFallbackArgs = new OnFallbackArguments<T>(context, outcome);

        _telemetry.Report<OnFallbackArguments<T>, T>(new(ResilienceEventSeverity.Warning, FallbackConstants.OnFallback), onFallbackArgs);

        if (_onFallback is not null)
        {
            await _onFallback(onFallbackArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        try
        {
            return await _handler.ActionGenerator(new FallbackActionArguments<T>(context, outcome)).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            return Outcome.FromException<T>(e);
        }
    }
}
