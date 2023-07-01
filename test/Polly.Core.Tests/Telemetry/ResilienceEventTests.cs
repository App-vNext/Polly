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

    [Fact]
    public void Equality_Ok()
    {
        (new ResilienceEvent(ResilienceEventSeverity.Warning, "A") == new ResilienceEvent(ResilienceEventSeverity.Warning, "A")).Should().BeTrue();
        (new ResilienceEvent(ResilienceEventSeverity.Warning, "A") != new ResilienceEvent(ResilienceEventSeverity.Warning, "A")).Should().BeFalse();
        (new ResilienceEvent(ResilienceEventSeverity.Warning, "A") == new ResilienceEvent(ResilienceEventSeverity.Warning, "B")).Should().BeFalse();
        (new ResilienceEvent(ResilienceEventSeverity.Information, "A") == new ResilienceEvent(ResilienceEventSeverity.Warning, "A")).Should().BeFalse();
    }
}
