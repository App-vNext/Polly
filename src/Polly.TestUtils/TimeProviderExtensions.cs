namespace Polly.TestUtils;

public static class TimeProviderExtensions
{
    public static Task DelayAsync(this TimeProvider timeProvider, TimeSpan delay, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        return Task.Delay(delay, timeProvider, cancellationToken);
#else
        return timeProvider.Delay(delay, cancellationToken);
#endif
    }
}
