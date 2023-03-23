using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaultValues()
    {
        var options = new TimeoutStrategyOptions();

        options.TimeoutGenerator.Should().NotBeNull();
        options.OnTimeout.Should().NotBeNull();
        options.StrategyType.Should().Be(TimeoutConstants.StrategyType);
    }
}
