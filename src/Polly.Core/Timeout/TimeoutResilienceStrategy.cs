using System;
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

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
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

        CancellationTokenRegistration? registration = null;

        if (previousToken.CanBeCanceled)
        {
            registration = previousToken.Register(static state => ((CancellationTokenSource)state!).Cancel(), cancellationSource, useSynchronizationContext: false);
        }

        try
        {
            var result = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

            await DisposeRegistration(registration).ConfigureAwait(context.ContinueOnCapturedContext);

            return result;
        }
        catch (OperationCanceledException e) when (cancellationSource.IsCancellationRequested && !previousToken.IsCancellationRequested)
        {
            context.CancellationToken = previousToken;

            var args = new OnTimeoutArguments(context, e, timeout);
            _telemetry.Report(TimeoutConstants.OnTimeoutEvent, args);

            if (OnTimeout != null)
            {
                await OnTimeout(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            await DisposeRegistration(registration).ConfigureAwait(context.ContinueOnCapturedContext);

            throw new TimeoutRejectedException(
                $"The operation didn't complete within the allowed timeout of '{timeout}'.",
                timeout,
                e);
        }
        finally
        {
            context.CancellationToken = previousToken;
            _cancellationTokenSourcePool.Return(cancellationSource);
        }
    }

    internal ValueTask<TimeSpan> GetTimeoutAsync(ResilienceContext context)
    {
        if (TimeoutGenerator == null)
        {
            return new ValueTask<TimeSpan>(DefaultTimeout);
        }

        return TimeoutGenerator(new TimeoutGeneratorArguments(context));
    }

    private static ValueTask DisposeRegistration(CancellationTokenRegistration? registration)
    {
        if (registration.HasValue)
        {
#if NETCOREAPP
            return registration.Value.DisposeAsync();
#else
            registration.Value.Dispose();
#endif
        }

        return default;
    }
}
