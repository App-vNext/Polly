using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class ResilienceEventTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var ev = new ResilienceEvent(ResilienceEventSeverity.Warning, "A");

        ev.ToString().Should().Be("A");
        ev.Severity.Should().Be(ResilienceEventSeverity.Warning);
    }
}
