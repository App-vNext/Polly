using System;
using Polly.Strategy;

namespace Polly.Retry;

internal class RetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly RandomUtil _randomUtil;

    public RetryResilienceStrategy(RetryStrategyOptions options, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry, RandomUtil randomUtil)
    {
        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _randomUtil = randomUtil;
        OnRetry = options.OnRetry.CreateHandler();
        DelayGenerator = options.RetryDelayGenerator.CreateHandler(TimeSpan.MinValue, RetryHelper.IsValidDelay);
        ShouldRetry = options.ShouldRetry.CreateHandler();

        BackoffType = options.BackoffType;
        BaseDelay = options.BaseDelay;
        RetryCount = options.RetryCount;
    }

    public TimeSpan BaseDelay { get; }

    public RetryBackoffType BackoffType { get; }

    public int RetryCount { get; }

    public OutcomePredicate<ShouldRetryArguments>.Handler? ShouldRetry { get; }

    public OutcomeGenerator<RetryDelayArguments, TimeSpan>.Handler? DelayGenerator { get; }

    public OutcomeEvent<OnRetryArguments>.Handler? OnRetry { get; }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        double retryState = 0;

        if (ShouldRetry == null)
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

                if (IsLastAttempt(attempt) || !await ShouldRetry.ShouldHandleAsync(outcome, new ShouldRetryArguments(context, attempt)).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                outcome = new Outcome<TResult>(e);

                if (IsLastAttempt(attempt) || !await ShouldRetry.ShouldHandleAsync(outcome, new ShouldRetryArguments(context, attempt)).ConfigureAwait(context.ContinueOnCapturedContext))
                {
                    throw;
                }
            }

            var delay = RetryHelper.GetRetryDelay(BackoffType, attempt, BaseDelay, ref retryState, _randomUtil);
            if (DelayGenerator != null)
            {
                var newDelay = await DelayGenerator.GenerateAsync(outcome, new RetryDelayArguments(context, attempt, delay)).ConfigureAwait(false);
                if (RetryHelper.IsValidDelay(newDelay))
                {
                    delay = newDelay;
                }
            }

            var args = new OnRetryArguments(context, attempt, delay);

            _telemetry.Report(RetryConstants.OnRetryEvent, outcome, args);

            if (OnRetry != null)
            {
                await OnRetry.HandleAsync(outcome, args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            if (outcome.TryGetResult(out var resultValue))
            {
                await DisposeHelper.TryDisposeSafeAsync(resultValue, context.IsSynchronous).ConfigureAwait(context.ContinueOnCapturedContext);
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
