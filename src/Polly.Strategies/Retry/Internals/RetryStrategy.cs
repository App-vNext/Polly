using Polly.Internals;

namespace Polly.Retry;

internal sealed class RetryStrategy : DelegatingResilienceStrategy
{
    private readonly RetryStrategyOptions _options;
    private readonly PredicatesHandler<ShouldRetryArguments> _shouldRetry;
    private readonly EventsHandler<RetryActionArguments> _onRetry;
    private readonly int _maxRetries;

    public RetryStrategy(RetryStrategyOptions options)
    {
        _options = options;
        _shouldRetry = PredicatesHandler<ShouldRetryArguments>.Create(options.ShouldRetry);
        _onRetry = EventsHandler<RetryActionArguments>.Create(options.OnRetry);

        _maxRetries = options.RetryCount;
    }

    public async override ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state)
    {
        var tryCount = 0;

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            bool canRetry;
            Outcome<T> outcome;

            try
            {
                var result = await base.ExecuteAsync(execution, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                outcome = new Outcome<T>(result);

                if (!await _shouldRetry.HandleAsync(outcome, new ShouldRetryArguments(tryCount, context), context).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    return result;
                }

                canRetry = tryCount < _maxRetries;
                if (!canRetry)
                {
                    return result;
                }

                return result;
            }
            catch (Exception e)
            {
                if (!await _shouldRetry.HandleAsync(new Outcome<T>(e), new ShouldRetryArguments(tryCount, context), context).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    throw;
                }

                canRetry = tryCount < _maxRetries;
                if (!canRetry)
                {
                    throw;
                }

                outcome = new Outcome<T>(e);
            }

            if (tryCount < int.MaxValue)
            {
                tryCount++;
            }

            var waitDuration = _options.RetryDelayGenerator(tryCount);

            await _onRetry.HandleAsync(outcome, new RetryActionArguments(tryCount, context, waitDuration), context).ConfigureAwait(context.ContinueOnCapturedContext);

            await SleepAsync(context, waitDuration).ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }

    internal static Task SleepAsync(ResilienceContext context, TimeSpan delay)
    {
        if (delay == TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        if (context.IsSynchronous)
        {
            Thread.Sleep(delay);
            return Task.CompletedTask;
        }
        else
        {
            return Task.Delay(delay, context.CancellationToken);
        }
    }
}
