namespace Polly.Timeout;

public readonly struct OnTimeoutArguments
{
    public OnTimeoutArguments(ResilienceContext context) => Context = context;

    public ResilienceContext Context { get; }
}

