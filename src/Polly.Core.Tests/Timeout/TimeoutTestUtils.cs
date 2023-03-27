using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public static class TimeoutTestUtils
{
    public static OnTimeoutArguments OnTimeoutArguments() => new(ResilienceContext.Get(), new InvalidOperationException(), TimeSpan.FromSeconds(1));

    public static TimeoutGeneratorArguments TimeoutGeneratorArguments() => new(ResilienceContext.Get());

    public static readonly TheoryData<TimeSpan> InvalidTimeouts = new()
    {
        TimeSpan.MinValue,
        TimeSpan.Zero,
        TimeSpan.FromSeconds(-1)
    };

    public static readonly TheoryData<TimeSpan> ValidTimeouts = new()
    {
        TimeoutStrategyOptions.InfiniteTimeout,
        System.Threading.Timeout.InfiniteTimeSpan,
        TimeSpan.FromSeconds(1)
    };
}
