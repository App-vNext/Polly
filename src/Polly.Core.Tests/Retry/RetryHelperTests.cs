using System;
using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryHelperTests
{
    [Fact]
    public void UnsupportedRetryBackoffType_Throws()
    {
        RetryBackoffType type = (RetryBackoffType)99;

        Assert.Throws<NotSupportedException>(() => RetryHelper.GetRetryDelay(type, 0, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Constant_Ok()
    {
        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 0, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 1, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 2, TimeSpan.Zero).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 0, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 1, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(RetryBackoffType.Constant, 2, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Linear_Ok()
    {
        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 0, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 1, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 2, TimeSpan.Zero).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 0, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 1, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(2));
        RetryHelper.GetRetryDelay(RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Exponential_Ok()
    {
        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 0, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 1, TimeSpan.Zero).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 2, TimeSpan.Zero).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 0, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 1, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(2));
        RetryHelper.GetRetryDelay(RetryBackoffType.Exponential, 2, TimeSpan.FromSeconds(1)).Should().Be(TimeSpan.FromSeconds(4));
    }
}
