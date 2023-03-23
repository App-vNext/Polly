using System;
using System.Threading.Tasks;
using Polly.Telemetry;

namespace Polly.Timeout;

internal sealed class TimeoutResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceTelemetry _telemetry;

    public TimeoutResilienceStrategy(TimeoutStrategyOptions options, TimeProvider timeProvider, ResilienceTelemetry telemetry)
    {
        TimeoutGenerator = options.TimeoutGenerator.CreateHandler();
        OnTimeout = options.OnTimeout.CreateHandler();
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    public Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>> TimeoutGenerator { get; }

    public Func<OnTimeoutArguments, ValueTask>? OnTimeout { get; }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        var timeout = await TimeoutGenerator(new TimeoutGeneratorArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        if (!TimeoutUtil.ShouldApplyTimeout(timeout))
        {
            // do nothing
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var previousToken = context.CancellationToken;
        using var cancellationSource = new CancellationTokenSource();
        _timeProvider.CancelAfter(cancellationSource, timeout);
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
            _telemetry.Report(TimeoutConstants.OnTimeoutEvent, context);

            if (OnTimeout != null)
            {
                await OnTimeout(new OnTimeoutArguments(context, e, timeout)).ConfigureAwait(context.ContinueOnCapturedContext);
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
        }
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
