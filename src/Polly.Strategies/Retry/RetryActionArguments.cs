namespace Polly.Retry;

public readonly struct RetryActionArguments
{
    public RetryActionArguments(int attemptNumber, ResilienceContext context, TimeSpan waitingTimeInterval)
    {
        AttemptNumber = attemptNumber;
        Context = context;
        WaitingTimeInterval = waitingTimeInterval;
    }

    public int AttemptNumber { get; }

    public TimeSpan WaitingTimeInterval { get; }

    public ResilienceContext Context { get; }

    public CancellationToken CancellationToken => Context.CancellationToken;
}

