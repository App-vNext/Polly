using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryHelperTests
{
    private Func<double> _randomizer = new RandomUtil(0).NextDouble;

    public static TheoryData<int> Attempts()
    {
#pragma warning disable IDE0028
        return new()
        {
            1,
            2,
            3,
            4,
            10,
            100,
            1_000,
            1_024,
            1_025,
        };
#pragma warning restore IDE0028
    }

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
            return RetryHelper.GetRetryDelay(type, jitter, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer);
        });
    }

    [Fact]
    public void Constant_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 0, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 1, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 2, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 1, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constant_Jitter_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, true, 0, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, true, 1, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, true, 2, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        _randomizer = () => 0.0;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Constant, true, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(0.75));

        _randomizer = () => 0.4;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Constant, true, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(0.95));

        _randomizer = () => 0.6;
        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, true, 1, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(1.05));

        _randomizer = () => 1.0;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Constant, true, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(1.25));
    }

    [Fact]
    public void Linear_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 0, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 1, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 2, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 1, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(2));
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, false, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Linear_Jitter_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, true, 0, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, true, 1, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        _randomizer = () => 0.0;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(2.25));

        _randomizer = () => 0.4;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(2.85));

        _randomizer = () => 0.5;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(3));

        _randomizer = () => 0.6;
        RetryHelper.GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(3.15));

        _randomizer = () => 1.0;
        RetryHelper
            .GetRetryDelay(DelayBackoffType.Linear, true, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer)
            .Should()
            .Be(TimeSpan.FromSeconds(3.75));
    }

    [Fact]
    public void Exponential_Ok()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 0, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 2, TimeSpan.Zero, null, ref state, _randomizer).Should().Be(TimeSpan.Zero);

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 0, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(1));
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(2));
        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 2, TimeSpan.FromSeconds(1), null, ref state, _randomizer).Should().Be(TimeSpan.FromSeconds(4));
    }

    [InlineData(DelayBackoffType.Linear, false)]
    [InlineData(DelayBackoffType.Exponential, false)]
    [InlineData(DelayBackoffType.Constant, false)]
    [InlineData(DelayBackoffType.Linear, true)]
    [InlineData(DelayBackoffType.Exponential, true)]
    [InlineData(DelayBackoffType.Constant, true)]
    [Theory]
    public void MaxDelay_Ok(DelayBackoffType type, bool jitter)
    {
        _randomizer = () => 0.5;
        var expected = TimeSpan.FromSeconds(1);
        double state = 0;

        RetryHelper.GetRetryDelay(type, jitter, 2, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), ref state, _randomizer).Should().Be(expected);
    }

    [Fact]
    public void MaxDelay_DelayLessThanMaxDelay_Respected()
    {
        double state = 0;
        var expected = TimeSpan.FromSeconds(1);

        RetryHelper.GetRetryDelay(DelayBackoffType.Constant, false, 2, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), ref state, _randomizer).Should().Be(expected);
    }

    [Fact]
    public void GetRetryDelay_Overflow_ReturnsMaxTimeSpan()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1000, TimeSpan.FromDays(1), null, ref state, _randomizer).Should().Be(TimeSpan.MaxValue);
    }

    [Fact]
    public void GetRetryDelay_OverflowWithMaxDelay_ReturnsMaxDelay()
    {
        double state = 0;

        RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, false, 1000, TimeSpan.FromDays(1), TimeSpan.FromDays(2), ref state, _randomizer).Should().Be(TimeSpan.FromDays(2));
    }

    [Theory]
    [MemberData(nameof(Attempts))]
    public void GetRetryDelay_Exponential_Is_Positive_When_No_Maximum_Delay(int attempt)
    {
        var jitter = true;
        var type = DelayBackoffType.Exponential;

        var baseDelay = TimeSpan.FromSeconds(2);
        TimeSpan? maxDelay = null;

        var random = new RandomUtil(0).NextDouble;
        double state = 0;

        var first = RetryHelper.GetRetryDelay(type, jitter, attempt, baseDelay, maxDelay, ref state, random);
        var second = RetryHelper.GetRetryDelay(type, jitter, attempt, baseDelay, maxDelay, ref state, random);

        first.Should().BePositive();
        second.Should().BePositive();
    }

    [Theory]
    [MemberData(nameof(Attempts))]
    public void GetRetryDelay_Exponential_Does_Not_Exceed_MaxDelay(int attempt)
    {
        var jitter = true;
        var type = DelayBackoffType.Exponential;

        var baseDelay = TimeSpan.FromSeconds(2);
        var maxDelay = TimeSpan.FromSeconds(30);

        var random = new RandomUtil(0).NextDouble;
        double state = 0;

        var first = RetryHelper.GetRetryDelay(type, jitter, attempt, baseDelay, maxDelay, ref state, random);
        var second = RetryHelper.GetRetryDelay(type, jitter, attempt, baseDelay, maxDelay, ref state, random);

        first.Should().BePositive();
        first.Should().BeLessThanOrEqualTo(maxDelay);

        second.Should().BePositive();
        second.Should().BeLessThanOrEqualTo(maxDelay);
    }

    [Theory]
    [MemberData(nameof(Attempts))]
    public void ExponentialWithJitter_Ok(int count)
    {
        var delay = TimeSpan.FromSeconds(7.8);
        var oldDelays = GetExponentialWithJitterBackoff(true, delay, count);
        var newDelays = GetExponentialWithJitterBackoff(false, delay, count);

        newDelays.Should().ContainInConsecutiveOrder(oldDelays);
        newDelays.Should().HaveCount(oldDelays.Count);
        newDelays.Should().AllSatisfy(delay => delay.Should().BePositive());
    }

    [Fact]
    public void ExponentialWithJitter_EnsureRandomness()
    {
        var delay = TimeSpan.FromSeconds(7.8);
        var delays1 = GetExponentialWithJitterBackoff(false, delay, 100, RandomUtil.Instance.NextDouble);
        var delays2 = GetExponentialWithJitterBackoff(false, delay, 100, RandomUtil.Instance.NextDouble);

        delays1.SequenceEqual(delays2).Should().BeFalse();
        delays1.Should().AllSatisfy(delay => delay.Should().BePositive());
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
            result.Add(RetryHelper.GetRetryDelay(DelayBackoffType.Exponential, true, i, baseDelay, null, ref state, random));
        }

        return result;
    }
}
