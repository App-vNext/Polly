using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        RetryConstants.DefaultBackoffType.Should().Be(RetryBackoffType.Constant);
        RetryConstants.DefaultBaseDelay.Should().Be(TimeSpan.FromSeconds(2));
        RetryConstants.DefaultRetryCount.Should().Be(3);
        RetryConstants.MaxRetryCount.Should().Be(100);
        RetryConstants.InfiniteRetryCount.Should().Be(-1);
        RetryConstants.StrategyType.Should().Be("Retry");
        RetryConstants.OnRetryEvent.Should().Be("OnRetry");
    }
}
