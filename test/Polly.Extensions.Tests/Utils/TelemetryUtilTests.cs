using Polly.Telemetry;

namespace Polly.Extensions.Tests.Utils;

public class TelemetryUtilTests
{
    [Fact]
    public void AsBoxedPool_Ok()
    {
        TelemetryUtil.AsBoxedBool(true).ShouldBe(true);
        TelemetryUtil.AsBoxedBool(false).ShouldBe(false);
    }

    [Fact]
    public void AsBoxedInt_Ok()
    {
        var hash = new HashSet<object>();

        for (int i = 0; i < 100; i++)
        {
            i.AsBoxedInt().ShouldBeSameAs(i.AsBoxedInt());
            i.AsBoxedInt().ShouldBe(i);
        }

        (-1).AsBoxedInt().ShouldNotBeSameAs((-1).AsBoxedInt());
        100.AsBoxedInt().ShouldNotBeSameAs(100.AsBoxedInt());
    }

    [Fact]
    public void GetValueOrPlaceholder_Ok()
    {
        TelemetryUtil.GetValueOrPlaceholder("dummy").ShouldBe("dummy");
        TelemetryUtil.GetValueOrPlaceholder(null).ShouldBe("(null)");
    }
}
