using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class ReportedResilienceEventTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var ev = new ReportedResilienceEvent("A");

        ev.ToString().Should().Be("A");
    }

    [Fact]
    public void Equality_Ok()
    {
        (new ReportedResilienceEvent("A") == new ReportedResilienceEvent("A")).Should().BeTrue();
        (new ReportedResilienceEvent("A") != new ReportedResilienceEvent("A")).Should().BeFalse();
        (new ReportedResilienceEvent("A") == new ReportedResilienceEvent("B")).Should().BeFalse();
    }
}
