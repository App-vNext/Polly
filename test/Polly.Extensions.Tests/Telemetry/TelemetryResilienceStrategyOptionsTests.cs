using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryResilienceStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new TelemetryResilienceStrategyOptions();

        options.Enrichers.Should().BeEmpty();
        options.LoggerFactory.Should().Be(NullLoggerFactory.Instance);
        var resilienceContext = ResilienceContext.Get();
        options.ResultFormatter(resilienceContext, null).Should().BeNull();
        options.ResultFormatter(resilienceContext, "dummy").Should().Be("dummy");

        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        options.ResultFormatter(resilienceContext, response).Should().Be(200);
    }
}
