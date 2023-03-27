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
                NullResilienceTelemetry.Instance.Report("dummy", 10, ResilienceContext.Get());
                NullResilienceTelemetry.Instance.ReportException("dummy", new InvalidOperationException(), ResilienceContext.Get());
            })
            .Should()
            .NotThrow();
    }
}
