using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        RetryConstants.DefaultBackoffType.Should().Be(DelayBackoffType.Constant);
        RetryConstants.DefaultBaseDelay.Should().Be(TimeSpan.FromSeconds(2));
        RetryConstants.DefaultRetryCount.Should().Be(3);
        RetryConstants.MaxRetryCount.Should().Be(int.MaxValue);
        RetryConstants.OnRetryEvent.Should().Be("OnRetry");
    }
}
