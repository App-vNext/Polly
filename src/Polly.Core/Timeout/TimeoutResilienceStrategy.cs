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

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeout = DefaultTimeout;
        if (TimeoutGenerator is not null)
        {
            timeout = await TimeoutGenerator!(new TimeoutGeneratorArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);
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

        Outcome<TResult> outcome;
        try
        {
            outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
#pragma warning disable CA1031
        catch (Exception ex)
        {
            outcome = new(ex);
        }
#pragma warning restore CA1031

        var isCancellationRequested = cancellationSource.IsCancellationRequested;

        // execution is finished, clean up
        context.CancellationToken = previousToken;
#pragma warning disable CA1849 // Call async methods when in an async method, OK here as the callback is synchronous
#pragma warning disable S6966
        registration.Dispose();
#pragma warning restore S6966
#pragma warning restore CA1849 // Call async methods when in an async method

        _cancellationTokenSourcePool.Return(cancellationSource);

        // check the outcome
        if (isCancellationRequested && outcome.Exception is OperationCanceledException e && !previousToken.IsCancellationRequested)
        {
            var args = new OnTimeoutArguments(context, timeout);
            _telemetry.Report(new(ResilienceEventSeverity.Error, TimeoutConstants.OnTimeoutEvent), context, args);

            if (OnTimeout is not null)
            {
                await OnTimeout(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            var timeoutException = new TimeoutRejectedException(
                $"The operation didn't complete within the allowed timeout of '{timeout}'.",
                timeout,
                e);

            _telemetry.SetTelemetrySource(timeoutException);
            return Outcome.FromException<TResult>(timeoutException.TrySetStackTrace());
        }

        return outcome;
    }

    private static CancellationTokenRegistration CreateRegistration(CancellationTokenSource cancellationSource, CancellationToken previousToken)
    {
#if NET
        return previousToken.UnsafeRegister(static state => ((CancellationTokenSource)state!).Cancel(), cancellationSource);
#else
        return previousToken.Register(static state => ((CancellationTokenSource)state!).Cancel(), cancellationSource);
#endif
    }
}
