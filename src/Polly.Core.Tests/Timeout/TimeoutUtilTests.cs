using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;
public class TimeoutUtilTests
{
    public static readonly TheoryData<TimeSpan, bool> ShouldApplyTimeoutData = new()
    {
        { TimeSpan.FromSeconds(-1), false },
        { TimeSpan.Zero, false },
        { TimeSpan.FromSeconds(1), true },
        { System.Threading.Timeout.InfiniteTimeSpan, false },
    };

    public static readonly TheoryData<TimeSpan, bool> ValidateData = new()
    {
        { TimeSpan.FromSeconds(-1), false },
        { TimeSpan.Zero, false },
        { TimeSpan.FromSeconds(1), true },
        { System.Threading.Timeout.InfiniteTimeSpan, true },
    };

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void ValidateTimeout_Invalid_Throws(TimeSpan timeSpan)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TimeoutUtil.ValidateTimeout(timeSpan));
    }

    [MemberData(nameof(TimeoutTestUtils.ValidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void ValidateTimeout_Valid_DoesNotThrow(TimeSpan timeSpan)
    {
        this.Invoking(_ => TimeoutUtil.ValidateTimeout(timeSpan)).Should().NotThrow();
    }

    [MemberData(nameof(ShouldApplyTimeoutData))]
    [Theory]
    public void ShouldApplyTimeout_Ok(TimeSpan timeSpan, bool result)
    {
        TimeoutUtil.ShouldApplyTimeout(timeSpan).Should().Be(result);
    }
}
