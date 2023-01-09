namespace Polly.Retry;

public readonly struct ShouldRetryArguments
{
    public ShouldRetryArguments(int attemptNumber, ResilienceContext context)
    {
        AttemptNumber = attemptNumber;
        Context = context;
    }

    public int AttemptNumber { get; }

    public ResilienceContext Context { get; }

    public CancellationToken CancellationToken => Context.CancellationToken;
}
