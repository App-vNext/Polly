using System;
using Polly.Timeout;
using Xunit;

namespace Polly.Core.Tests.Timeout;
public class TimeoutUtilTests
{
    public static readonly TheoryData<TimeSpan, bool> ShouldApplyTimeoutData = new()
    {
        { TimeSpan.FromSeconds(-1), false },
        { TimeSpan.Zero, false },
        { TimeSpan.FromSeconds(1), true },
        { System.Threading.Timeout.InfiniteTimeSpan, false },
        { TimeoutConstants.InfiniteTimeout, false },
    };

    public static readonly TheoryData<TimeSpan, bool> ValidateData = new()
    {
        { TimeSpan.FromSeconds(-1), false },
        { TimeSpan.Zero, false },
        { TimeSpan.FromSeconds(1), true },
        { System.Threading.Timeout.InfiniteTimeSpan, true },
        { TimeoutConstants.InfiniteTimeout, true },
    };

    [MemberData(nameof(ValidateData))]
    [Theory]
    public void Validate_Ok(TimeSpan timeSpan, bool valid)
    {
        if (valid)
        {
            TimeoutUtil.ValidateTimeout(timeSpan);
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeoutUtil.ValidateTimeout(timeSpan));
        }
    }

    [MemberData(nameof(ShouldApplyTimeoutData))]
    [Theory]
    public void ShouldApplyTimeout_Ok(TimeSpan timeSpan, bool result)
    {
        TimeoutUtil.ShouldApplyTimeout(timeSpan).Should().Be(result);
    }
}
