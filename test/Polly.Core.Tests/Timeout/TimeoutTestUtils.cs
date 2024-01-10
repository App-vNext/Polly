using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public static class TimeoutTestUtils
{
    public static OnTimeoutArguments OnTimeoutArguments() => new(ResilienceContextPool.Shared.Get(), TimeSpan.FromSeconds(1));

    public static TimeoutGeneratorArguments TimeoutGeneratorArguments() => new(ResilienceContextPool.Shared.Get());

#pragma warning disable IDE0028
    public static readonly TheoryData<TimeSpan> InvalidTimeouts = new()
    {
        TimeSpan.MinValue,
        TimeSpan.Zero,
        TimeSpan.FromSeconds(-1),
        TimeSpan.FromHours(25),
        TimeSpan.FromMilliseconds(9),
    };

    public static readonly TheoryData<TimeSpan> ValidTimeouts = new()
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromHours(1),
        TimeSpan.FromMilliseconds(10),

    };
#pragma warning restore IDE0028
}
