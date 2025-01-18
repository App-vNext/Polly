using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        RetryConstants.DefaultBackoffType.ShouldBe(DelayBackoffType.Constant);
        RetryConstants.DefaultBaseDelay.ShouldBe(TimeSpan.FromSeconds(2));
        RetryConstants.DefaultRetryCount.ShouldBe(3);
        RetryConstants.MaxRetryCount.ShouldBe(int.MaxValue);
        RetryConstants.OnRetryEvent.ShouldBe("OnRetry");
    }
}
