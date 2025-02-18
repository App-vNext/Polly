using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;
public class TimeoutUtilTests
{
    public static readonly List<(TimeSpan, bool)> ShouldApplyTimeoutData = new()
    {
        ( TimeSpan.FromSeconds(-1), false ),
        ( TimeSpan.Zero, false ),
        ( TimeSpan.FromSeconds(1), true ),
        ( System.Threading.Timeout.InfiniteTimeSpan, false ),
    };

    public static readonly TheoryData<TimeSpan, bool> ValidateData = new()
    {
        { TimeSpan.FromSeconds(-1), false },
        { TimeSpan.Zero, false },
        { TimeSpan.FromSeconds(1), true },
        { System.Threading.Timeout.InfiniteTimeSpan, true },
    };

    //[MemberData(nameof(ShouldApplyTimeoutData))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [Theory]
    public void ShouldApplyTimeout_Ok(int index)
    {
        (TimeSpan timeSpan, bool result) = ShouldApplyTimeoutData[index];
        TimeoutUtil.ShouldApplyTimeout(timeSpan).ShouldBe(result);
    }
}
