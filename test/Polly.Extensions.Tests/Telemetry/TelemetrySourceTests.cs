using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public static class TelemetrySourceTests
{
    [Fact]
    public static void TelemetrySource_CreatesActivitySourceAndMeter()
    {
        var source = TelemetrySource.Instance;

        source.ShouldNotBeNull();

        source.Meter.ShouldNotBeNull();
        source.Meter.Name.ShouldBe("Polly");
        source.Meter.Version.ShouldNotBeNullOrEmpty();
        Version.TryParse(source.Meter.Version, out _).ShouldBeTrue();
    }
}
