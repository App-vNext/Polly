using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public static class TimeoutTestUtils
{
    public static OnTimeoutArguments OnTimeoutArguments() => new(ResilienceContext.Get(), new InvalidOperationException(), TimeSpan.FromSeconds(1));

    public static TimeoutGeneratorArguments TimeoutGeneratorArguments() => new(ResilienceContext.Get());
}
