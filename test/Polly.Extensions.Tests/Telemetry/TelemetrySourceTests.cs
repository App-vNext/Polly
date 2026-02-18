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
        source.Meter.Version.ShouldNotContain('-');
        source.Meter.Version.ShouldNotContain('+');
        Version.TryParse(source.Meter.Version, out var version).ShouldBeTrue();
        version.ShouldBeGreaterThan(new(0, 0, 0));

        source.ActivitySource.ShouldNotBeNull();
        source.ActivitySource.Name.ShouldBe("Polly");
        source.ActivitySource.Version.ShouldNotBeNullOrEmpty();
        source.ActivitySource.Version.ShouldNotContain('-');
        source.ActivitySource.Version.ShouldNotContain('+');
        Version.TryParse(source.ActivitySource.Version, out version).ShouldBeTrue();
        version.ShouldBeGreaterThan(new(0, 0, 0));
    }
}
