using System.Diagnostics.CodeAnalysis;
using Polly.Telemetry;

namespace Polly.Timeout;

internal sealed class TimeoutResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly CancellationTokenSourcePool _cancellationTokenSourcePool;

    public TimeoutResilienceStrategy(TimeoutStrategyOptions options, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry)
    {
        DefaultTimeout = options.Timeout;
        TimeoutGenerator = options.TimeoutGenerator;
        OnTimeout = options.OnTimeout;
        _telemetry = telemetry;
        _cancellationTokenSourcePool = CancellationTokenSourcePool.Create(timeProvider);
    }

    public TimeSpan DefaultTimeout { get; }

    public Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? TimeoutGenerator { get; }

    public Func<OnTimeoutArguments, ValueTask>? OnTimeout { get; }

    [ExcludeFromCodeCoverage]
    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeout = DefaultTimeout;
        if (TimeoutGenerator is not null)
        {
            timeout = await GenerateTimeoutAsync(context).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        if (!TimeoutUtil.ShouldApplyTimeout(timeout))
        {
            // do nothing
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var previousToken = context.CancellationToken;
        var cancellationSource = _cancellationTokenSourcePool.Get(timeout);
        context.CancellationToken = cancellationSource.Token;

        var registration = CreateRegistration(cancellationSource, previousToken);

        var outcome = await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var isCancellationRequested = cancellationSource.IsCancellationRequested;

        // execution is finished, cleanup
        context.CancellationToken = previousToken;
#pragma warning disable CA1849 // Call async methods when in an async method, OK here as the callback is synchronous
        registration.Dispose();
#pragma warning restore CA1849 // Call async methods when in an async method

        _cancellationTokenSourcePool.Return(cancellationSource);

        // check the outcome
        if (isCancellationRequested && outcome.Exception is OperationCanceledException e && !previousToken.IsCancellationRequested)
        {
            var args = new OnTimeoutArguments(context, e, timeout);
            _telemetry.Report(TimeoutConstants.OnTimeoutEvent, context, args);

            if (OnTimeout is not null)
            {
                await OnTimeout(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            var timeoutException = new TimeoutRejectedException(
                $"The operation didn't complete within the allowed timeout of '{timeout}'.",
                timeout,
                e);

            return Outcome.FromException<TResult>(timeoutException.TrySetStackTrace());
        }

        return outcome;
    }

    internal async ValueTask<TimeSpan> GenerateTimeoutAsync(ResilienceContext context)
    {
        var timeout = await TimeoutGenerator!(new TimeoutGeneratorArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        if (!TimeoutUtil.IsTimeoutValid(timeout))
        {
            return DefaultTimeout;
        }

        return timeout;
    }

    private static CancellationTokenRegistration CreateRegistration(CancellationTokenSource cancellationSource, CancellationToken previousToken)
    {
        if (previousToken.CanBeCanceled)
        {
            return previousToken.Register(static state => ((CancellationTokenSource)state!).Cancel(), cancellationSource, useSynchronizationContext: false);
        }

        return default;
    }
}
