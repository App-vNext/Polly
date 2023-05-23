using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Timeout;

internal sealed class TimeoutResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly CancellationTokenSourcePool _cancellationTokenSourcePool;

    public TimeoutResilienceStrategy(TimeoutStrategyOptions options, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry)
    {
        DefaultTimeout = options.Timeout;
        TimeoutGenerator = options.TimeoutGenerator.CreateHandler(DefaultTimeout, TimeoutUtil.IsTimeoutValid);
        OnTimeout = options.OnTimeout.CreateHandler();
        _telemetry = telemetry;
        _cancellationTokenSourcePool = CancellationTokenSourcePool.Create(timeProvider);
    }

    public TimeSpan DefaultTimeout { get; }

    public Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? TimeoutGenerator { get; }

    public Func<OnTimeoutArguments, ValueTask>? OnTimeout { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeout = await GetTimeoutAsync(context).ConfigureAwait(context.ContinueOnCapturedContext);

        if (!TimeoutUtil.ShouldApplyTimeout(timeout))
        {
            // do nothing
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var previousToken = context.CancellationToken;
        var cancellationSource = _cancellationTokenSourcePool.Get(timeout);
        context.CancellationToken = cancellationSource.Token;

#if NETCOREAPP
        await using var registration = CreateRegistration(cancellationSource, previousToken).ConfigureAwait(context.ContinueOnCapturedContext);
#else
        using var registration = CreateRegistration(cancellationSource, previousToken);
#endif
        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        var isCancellationRequested = cancellationSource.IsCancellationRequested;

        // execution is finished, cleanup
        context.CancellationToken = previousToken;
        _cancellationTokenSourcePool.Return(cancellationSource);

        // check the outcome
        if (outcome.Exception is OperationCanceledException e && isCancellationRequested && !previousToken.IsCancellationRequested)
        {
            var args = new OnTimeoutArguments(context, e, timeout);
            _telemetry.Report(TimeoutConstants.OnTimeoutEvent, args);

            if (OnTimeout != null)
            {
                await OnTimeout(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            var timeoutException = new TimeoutRejectedException(
                $"The operation didn't complete within the allowed timeout of '{timeout}'.",
                timeout,
                e);

            return new Outcome<TResult>(ExceptionDispatchInfo.Capture(timeoutException));
        }

        return outcome;
    }

    internal ValueTask<TimeSpan> GetTimeoutAsync(ResilienceContext context)
    {
        if (TimeoutGenerator == null)
        {
            return new ValueTask<TimeSpan>(DefaultTimeout);
        }

        return TimeoutGenerator(new TimeoutGeneratorArguments(context));
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
