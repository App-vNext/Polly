using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class NullResilienceTelemetryTests
{
    [Fact]
    public void Instance_NotNull()
    {
        NullResilienceTelemetry.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Report_ShouldNotThrow()
    {
        NullResilienceTelemetry.Instance
            .Invoking(v =>
            {
                NullResilienceTelemetry.Instance.Report("dummy", ResilienceContext.Get());
                NullResilienceTelemetry.Instance.Report("dummy", new Outcome<int>(10), ResilienceContext.Get());
            })
            .Should()
            .NotThrow();
    }
}
