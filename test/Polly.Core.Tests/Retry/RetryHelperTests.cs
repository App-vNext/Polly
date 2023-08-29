using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryHelperTests
{
    private Func<double> _randomizer = new RandomUtil(0).NextDouble;

    [Fact]
    public void IsValidDelay_Ok()
    {
        RetryHelper.IsValidDelay(TimeSpan.Zero).Should().BeTrue();
        RetryHelper.IsValidDelay(TimeSpan.FromSeconds(1)).Should().BeTrue();
        RetryHelper.IsValidDelay(TimeSpan.MaxValue).Should().BeTrue();
        RetryHelper.IsValidDelay(TimeSpan.MinValue).Should().BeFalse();
        RetryHelper.IsValidDelay(TimeSpan.FromMilliseconds(-1)).Should().BeFalse();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void UnsupportedRetryBackoffType_Throws(bool jitter)
    {
        DelayBackoffType type = (DelayBackoffType)99;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            double state = 0;
            return RetryHelper.GetRetryDelay(type, jitter, 0, TimeSpan.FromSeconds(1), ref state, _randomizer);
        });
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Constant_Ok(bool jitter)
    {
        double state = 0;
        if (jitter)
        {
            _randomizer = () => 0.5;
        }

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 0, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 1, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 2, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        var expected = !jitter ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(1.25);

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 0, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(expected);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 1, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(expected);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, jitter, 2, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(expected);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Linear_Ok(bool jitter)
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 0, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 1, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 2, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        if (jitter)
        {
            _randomizer = () => 0.5;

            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 0, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1.25));
            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 1, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(2.5));
            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 2, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(3.75));
        }
        else
        {
            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 0, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 1, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(2));
            RetryHelper.GetRetryDelay(DelayBackoffType.Linear, jitter, 2, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(3));
        }
    }

    [Fact]
    public void Exponential_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 0, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 2, TimeSpan.Zero, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 0, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(2));
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 2, TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(4));
    }

    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [Theory]
    public void ExponentialWithJitter_Ok(int count)
    {
        var delay = TimeSpan.FromSeconds(7.8);
        var oldDelays = GetExponentialWithJitterBackoff(true, delay, count);
        var newDelays = GetExponentialWithJitterBackoff(false, delay, count);

        newDelays.Should().ContainInConsecutiveOrder(oldDelays);
        newDelays.Should().HaveCount(oldDelays.Count);
    }

    [Fact]
    public void ExponentialWithJitter_EnsureRandomness()
    {
        var delay = TimeSpan.FromSeconds(7.8);
        var delays1 = GetExponentialWithJitterBackoff(false, delay, 100, RandomUtil.Instance.NextDouble);
        var delays2 = GetExponentialWithJitterBackoff(false, delay, 100, RandomUtil.Instance.NextDouble);

        delays1.SequenceEqual(delays2).Should().BeFalse();
    }

    private static IReadOnlyList<TimeSpan> GetExponentialWithJitterBackoff(bool contrib, TimeSpan baseDelay, int retryCount, Func<double>? randomizer = null)
    {
        if (contrib)
        {
            return Backoff.DecorrelatedJitterBackoffV2(baseDelay, retryCount, 0, false).Take(retryCount).ToArray();
        }

        var random = randomizer ?? new RandomUtil(0).NextDouble;
        double state = 0;
        var result = new List<TimeSpan>();

        for (int i = 0; i < retryCount; i++)
        {
            result.Add(RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, true, i, baseDelay, ref state, random));
        }

        return result;
    }
}
