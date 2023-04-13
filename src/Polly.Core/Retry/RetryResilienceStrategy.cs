using System;
using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Retry;

internal class RetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceTelemetry _telemetry;
    private readonly OutcomeEvent<OnRetryArguments, OnRetryEvent>.Handler? _onRetry;
    private readonly OutcomeGenerator<TimeSpan, RetryDelayArguments, RetryDelayGenerator>.Handler? _delayGenerator;
    private readonly OutcomePredicate<ShouldRetryArguments, ShouldRetryPredicate>.Handler? _shouldRetry;

    public RetryResilienceStrategy(RetryStrategyOptions options, TimeProvider timeProvider, ResilienceTelemetry telemetry)
    {
        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _onRetry = options.OnRetry.CreateHandler();
        _delayGenerator = options.RetryDelayGenerator.CreateHandler();
        _shouldRetry = options.ShouldRetry.CreateHandler();

        BackoffType = options.BackoffType;
        BaseDelay = options.BaseDelay;
        RetryCount = options.RetryCount;
    }

    public TimeSpan BaseDelay { get; }

    public RetryBackoffType BackoffType { get; }

    public int RetryCount { get; }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        if (_shouldRetry == null)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        int attempt = 0;

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            Outcome<TResult> outcome;

            try
            {
                var result = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                outcome = new Outcome<TResult>(result);

                if (IsLastAttempt(attempt) || !await _shouldRetry.ShouldHandle(outcome, new ShouldRetryArguments(context, attempt)).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                outcome = new Outcome<TResult>(e);

                if (IsLastAttempt(attempt) || !await _shouldRetry.ShouldHandle(outcome, new ShouldRetryArguments(context, attempt)).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    throw;
                }
            }

            var delay = RetryHelper.GetRetryDelay(BackoffType, attempt, BaseDelay);
            if (_delayGenerator != null)
            {
                var newDelay = await _delayGenerator.Generate(outcome, new RetryDelayArguments(context, attempt, delay)).ConfigureAwait(false);
                if (RetryHelper.IsValidDelay(newDelay))
                {
                    delay = newDelay;
                }
            }

            var args = new OnRetryArguments(context, attempt, delay);

            _telemetry.Report(RetryConstants.OnRetryEvent, outcome, args);

            if (_onRetry != null)
            {
                await _onRetry.Handle(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            if (delay > TimeSpan.Zero)
            {
                await _timeProvider.DelayAsync(delay, context).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            attempt++;
        }
    }

    private bool IsLastAttempt(int attempt)
    {
        if (RetryCount == RetryStrategyOptions.InfiniteRetryCount)
        {
            return false;
        }

        return attempt >= RetryCount;
    }
}
