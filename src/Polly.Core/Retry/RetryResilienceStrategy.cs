using Polly.Telemetry;

namespace Polly.Retry;

#pragma warning disable S107 // Methods should not have too many parameters

internal sealed class RetryResilienceStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly RandomUtil _randomUtil;

    public RetryResilienceStrategy(
        TimeSpan baseDelay,
        RetryBackoffType backoffType,
        int retryCount,
        PredicateInvoker<RetryPredicateArguments> shouldRetry,
        EventInvoker<OnRetryArguments>? onRetry,
        GeneratorInvoker<RetryDelayArguments, TimeSpan>? delayGenerator,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry,
        RandomUtil randomUtil)
    {
        BaseDelay = baseDelay;
        BackoffType = backoffType;
        RetryCount = retryCount;
        ShouldRetry = shouldRetry;
        OnRetry = onRetry;
        DelayGenerator = delayGenerator;

        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _randomUtil = randomUtil;

    }

    public TimeSpan BaseDelay { get; }

    public RetryBackoffType BackoffType { get; }

    public int RetryCount { get; }

    public PredicateInvoker<RetryPredicateArguments> ShouldRetry { get; }

    public GeneratorInvoker<RetryDelayArguments, TimeSpan>? DelayGenerator { get; }

    public EventInvoker<OnRetryArguments>? OnRetry { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        double retryState = 0;

        int attempt = 0;

        while (true)
        {
            var outcome = await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            var shouldRetryArgs = new OutcomeArguments<TResult, RetryPredicateArguments>(context, outcome, new RetryPredicateArguments(attempt));

            if (context.CancellationToken.IsCancellationRequested || IsLastAttempt(attempt) || !await ShouldRetry.HandleAsync(shouldRetryArgs).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                return outcome;
            }

            var delay = RetryHelper.GetRetryDelay(BackoffType, attempt, BaseDelay, ref retryState, _randomUtil);
            if (DelayGenerator is not null)
            {
                var delayArgs = new OutcomeArguments<TResult, RetryDelayArguments>(context, outcome, new RetryDelayArguments(attempt, delay));
                var newDelay = await DelayGenerator.HandleAsync(delayArgs).ConfigureAwait(false);
                if (RetryHelper.IsValidDelay(newDelay))
                {
                    delay = newDelay;
                }
            }

            var onRetryArgs = new OutcomeArguments<TResult, OnRetryArguments>(context, outcome, new OnRetryArguments(attempt, delay));
            _telemetry.Report(RetryConstants.OnRetryEvent, onRetryArgs);

            if (OnRetry is not null)
            {
                await OnRetry.HandleAsync(onRetryArgs).ConfigureAwait(context.ContinueOnCapturedContext);
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
                    return new Outcome<TResult>(e);
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
