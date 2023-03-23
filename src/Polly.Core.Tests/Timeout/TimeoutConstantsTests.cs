using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutConstantsTests
{
    [Fact]
    public void EnsureDefaultValues()
    {
        TimeoutConstants.InfiniteTimeout.Should().Be(System.Threading.Timeout.InfiniteTimeSpan);
        TimeoutConstants.OnTimeoutEvent.Should().Be("OnTimeout");
        TimeoutConstants.StrategyType.Should().Be("Timeout");
    }
}
