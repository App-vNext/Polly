using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new TelemetryOptions();

        options.MeteringEnrichers.Should().BeEmpty();
        options.LoggerFactory.Should().Be(NullLoggerFactory.Instance);
        var resilienceContext = ResilienceContextPool.Shared.Get();
        options.ResultFormatter(resilienceContext, null).Should().BeNull();
        options.ResultFormatter(resilienceContext, "dummy").Should().Be("dummy");

        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        options.ResultFormatter(resilienceContext, response).Should().Be(200);
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        var options = new TelemetryOptions
        {
            LoggerFactory = Substitute.For<ILoggerFactory>(),
            SeverityProvider = _ => ResilienceEventSeverity.Error,
            ResultFormatter = (_, _) => "x",
        };

        options.MeteringEnrichers.Add(Substitute.For<MeteringEnricher>());
        options.TelemetryListeners.Add(Substitute.For<TelemetryListener>());

        var other = new TelemetryOptions(options);

        other.ResultFormatter.Should().Be(options.ResultFormatter);
        other.LoggerFactory.Should().Be(options.LoggerFactory);
        other.SeverityProvider.Should().Be(options.SeverityProvider);
        other.MeteringEnrichers.Should().BeEquivalentTo(options.MeteringEnrichers);
        other.TelemetryListeners.Should().BeEquivalentTo(options.TelemetryListeners);

        other.TelemetryListeners.Should().NotBeSameAs(options.TelemetryListeners);
        other.MeteringEnrichers.Should().NotBeSameAs(options.MeteringEnrichers);
    }
}
