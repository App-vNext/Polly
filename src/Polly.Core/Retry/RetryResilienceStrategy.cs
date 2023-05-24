using System;
using Polly.Strategy;

namespace Polly.Retry;

internal sealed class RetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly RandomUtil _randomUtil;

    public RetryResilienceStrategy(RetryStrategyOptions options, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry, RandomUtil randomUtil)
    {
        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _randomUtil = randomUtil;
        OnRetry = options.OnRetry;
        DelayGenerator = options.RetryDelayGenerator;
        ShouldRetry = options.ShouldRetry!;

        BackoffType = options.BackoffType;
        BaseDelay = options.BaseDelay;
        RetryCount = options.RetryCount;
    }

    public TimeSpan BaseDelay { get; }

    public RetryBackoffType BackoffType { get; }

    public int RetryCount { get; }

    public Func<Outcome, ShouldRetryArguments, ValueTask<bool>> ShouldRetry { get; }

    public Func<Outcome, RetryDelayArguments, ValueTask<TimeSpan>>? DelayGenerator { get; }

    public Func<Outcome, OnRetryArguments, ValueTask>? OnRetry { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        double retryState = 0;

        int attempt = 0;

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            Outcome<TResult> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            if (IsLastAttempt(attempt) || !await ShouldRetry(outcome.AsOutcome(), new ShouldRetryArguments(context, attempt)).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                return outcome;
            }

            var delay = RetryHelper.GetRetryDelay(BackoffType, attempt, BaseDelay, ref retryState, _randomUtil);
            if (DelayGenerator is not null)
            {
                var newDelay = await DelayGenerator(outcome.AsOutcome(), new RetryDelayArguments(context, attempt, delay)).ConfigureAwait(false);
                if (RetryHelper.IsValidDelay(newDelay))
                {
                    delay = newDelay;
                }
            }

            var args = new OnRetryArguments(context, attempt, delay);

            _telemetry.Report(RetryConstants.OnRetryEvent, outcome, args);

            if (OnRetry is not null)
            {
                await OnRetry(outcome.AsOutcome(), args).ConfigureAwait(context.ContinueOnCapturedContext);
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
