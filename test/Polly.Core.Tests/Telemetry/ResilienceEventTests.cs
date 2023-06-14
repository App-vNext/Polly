using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class ResilienceEventTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var ev = new ResilienceEvent("A");

        ev.ToString().Should().Be("A");
    }

    [Fact]
    public void Equality_Ok()
    {
        (new ResilienceEvent("A") == new ResilienceEvent("A")).Should().BeTrue();
        (new ResilienceEvent("A") != new ResilienceEvent("A")).Should().BeFalse();
        (new ResilienceEvent("A") == new ResilienceEvent("B")).Should().BeFalse();
    }
}
