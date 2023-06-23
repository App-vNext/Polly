using System;
using Polly.Telemetry;

namespace Polly.Retry;

internal sealed class RetryResilienceStrategy<T> : OutcomeResilienceStrategy<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Func<double> _randomizer;

    public RetryResilienceStrategy(
        RetryStrategyOptions<T> options,
        bool isGeneric,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry,
        Func<double> randomizer)
        : base(isGeneric)
    {
        ShouldHandle = options.ShouldHandle;
        BaseDelay = options.BaseDelay;
        BackoffType = options.BackoffType;
        RetryCount = options.RetryCount;
        OnRetry = options.OnRetry;
        DelayGenerator = options.RetryDelayGenerator;

        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _randomizer = randomizer;
    }

    public TimeSpan BaseDelay { get; }

    public RetryBackoffType BackoffType { get; }

    public int RetryCount { get; }

    public Func<OutcomeArguments<T, RetryPredicateArguments>, ValueTask<bool>> ShouldHandle { get; }

    public Func<OutcomeArguments<T, RetryDelayArguments>, ValueTask<TimeSpan>>? DelayGenerator { get; }

    public Func<OutcomeArguments<T, OnRetryArguments>, ValueTask>? OnRetry { get; }

    protected override async ValueTask<Outcome<T>> ExecuteCallbackAsync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        double retryState = 0;

        int attempt = 0;

        while (true)
        {
            var startTimestamp = _timeProvider.GetTimestamp();
            var outcome = await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            var shouldRetryArgs = new OutcomeArguments<T, RetryPredicateArguments>(context, outcome, new RetryPredicateArguments(attempt));
            var handle = await ShouldHandle(shouldRetryArgs).ConfigureAwait(context.ContinueOnCapturedContext);
            var executionTime = _timeProvider.GetElapsedTime(startTimestamp);

            TelemetryUtil.ReportExecutionAttempt(_telemetry, context, outcome, attempt, executionTime, handle);

            if (context.CancellationToken.IsCancellationRequested || IsLastAttempt(attempt) || !handle)
            {
                return outcome;
            }

            var delay = RetryHelper.GetRetryDelay(BackoffType, attempt, BaseDelay, ref retryState, _randomizer);
            if (DelayGenerator is not null)
            {
                var delayArgs = new OutcomeArguments<T, RetryDelayArguments>(context, outcome, new RetryDelayArguments(attempt, delay));
                var newDelay = await DelayGenerator(delayArgs).ConfigureAwait(false);
                if (RetryHelper.IsValidDelay(newDelay))
                {
                    delay = newDelay;
                }
            }

            var onRetryArgs = new OutcomeArguments<T, OnRetryArguments>(context, outcome, new OnRetryArguments(attempt, delay, executionTime));
            _telemetry.Report(RetryConstants.OnRetryEvent, onRetryArgs);

            if (OnRetry is not null)
            {
                await OnRetry(onRetryArgs).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            if (outcome.TryGetResult(out var resultValue))
            {
                await DisposeHelper.TryDisposeSafeAsync(resultValue, context.IsSynchronous).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await _timeProvider.DelayAsync(delay, context).ConfigureAwait(context.ContinueOnCapturedContext);
                }
                catch (OperationCanceledException e)
                {
                    return Outcome.FromException<T>(e);
                }
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
