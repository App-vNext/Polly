using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutConstantsTests
{
    [Fact]
    public void EnsureDefaultValues() =>
        TimeoutConstants.OnTimeoutEvent.Should().Be("OnTimeout");
}
