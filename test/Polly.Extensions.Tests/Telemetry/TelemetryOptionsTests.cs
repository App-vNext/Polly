using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new TelemetryOptions();

        options.Enrichers.Should().BeEmpty();
        options.LoggerFactory.Should().Be(NullLoggerFactory.Instance);
        var resilienceContext = ResilienceContextPool.Shared.Get();
        options.ResultFormatter(resilienceContext, null).Should().BeNull();
        options.ResultFormatter(resilienceContext, "dummy").Should().Be("dummy");

        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        options.ResultFormatter(resilienceContext, response).Should().Be(200);
    }
}
