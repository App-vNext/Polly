using FluentAssertions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Utils;

public class TelemetryUtilTests
{
    [Fact]
    public void AsBoxedPool_Ok()
    {
        TelemetryUtil.AsBoxedBool(true).Should().Be(true);
        TelemetryUtil.AsBoxedBool(false).Should().Be(false);
    }

    [Fact]
    public void AsBoxedInt_Ok()
    {
        var hash = new HashSet<object>();

        for (int i = 0; i < 100; i++)
        {
            i.AsBoxedInt().Should().BeSameAs(i.AsBoxedInt());
            i.AsBoxedInt().Should().Be(i);
        }

        (-1).AsBoxedInt().Should().NotBeSameAs((-1).AsBoxedInt());
        100.AsBoxedInt().Should().NotBeSameAs(100.AsBoxedInt());
    }
}
