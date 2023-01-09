using Microsoft.Extensions.ObjectPool;
using Polly.Internals;

namespace Polly.Timeout;

internal class TimeoutStrategy : DelegatingResilienceStrategy
{
    private static readonly ObjectPool<CancellationTokenSource> _cancellations = ObjectPool.Create<CancellationTokenSource>();

    private readonly TimeoutStrategyOptions _options;
    private readonly EventsHandler<TimeoutTaskArguments> _onTimeout;

    public TimeoutStrategy(TimeoutStrategyOptions options)
    {
        _options = options;
        _onTimeout = EventsHandler<TimeoutTaskArguments>.Create(_options.OnTimeout);
    }

    public override async ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state)
    {
        var previous = context.CancellationToken;
        var source = _cancellations.Get();

        try
        {
            using var registration = context.CancellationToken.Register(t => ((CancellationTokenSource)t!).Cancel(), source);
            source.CancelAfter(_options.TimeoutInterval);
            context.CancellationToken = source.Token;

            try
            {
                return await base.ExecuteAsync(execution, context, state).ConfigureAwait(false);
            }
            catch (Exception e) when (source.IsCancellationRequested && !previous.IsCancellationRequested)
            {
                await _onTimeout.HandleAsync(new Outcome<T>(e), new TimeoutTaskArguments(context), context).ConfigureAwait(false);

                throw new TimeoutException();
            }
        }
        finally
        {
            context.CancellationToken = previous;
#if NET6_0_OR_GREATER
            if (source.TryReset())
            {
                _cancellations.Return(source);
            }
#endif
        }
    }
}
