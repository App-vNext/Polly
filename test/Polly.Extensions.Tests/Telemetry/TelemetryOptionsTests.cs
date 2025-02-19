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

        options.MeteringEnrichers.ShouldBeEmpty();
        options.LoggerFactory.ShouldBe(NullLoggerFactory.Instance);
        var resilienceContext = ResilienceContextPool.Shared.Get();
        options.ResultFormatter(resilienceContext, null).ShouldBeNull();
        options.ResultFormatter(resilienceContext, "dummy").ShouldBe("dummy");

        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        options.ResultFormatter(resilienceContext, response).ShouldBe(200);
    }

    [Fact]
    public void CopyCtor_OtherNull_Throws()
    {
        var options = new TelemetryOptions();
        Assert.Throws<ArgumentNullException>("other", () => new TelemetryOptions(null!));
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

        other.ResultFormatter.ShouldBe(options.ResultFormatter);
        other.LoggerFactory.ShouldBe(options.LoggerFactory);
        other.SeverityProvider.ShouldBe(options.SeverityProvider);
        other.MeteringEnrichers.ShouldBeEquivalentTo(options.MeteringEnrichers);
        other.TelemetryListeners.ShouldBeEquivalentTo(options.TelemetryListeners);
        other.TelemetryListeners.ShouldNotBeSameAs(options.TelemetryListeners);
        other.MeteringEnrichers.ShouldNotBeSameAs(options.MeteringEnrichers);

        typeof(TelemetryOptions).GetRuntimeProperties().Count().ShouldBe(5);
    }

    [Fact]
    public void CopyCtor_Reminder()
        => typeof(TelemetryOptions).GetRuntimeProperties().Count()
        .ShouldBe(5, "Make sure that when you increase this number, you also update the copy constructor to assign the new property.");
}
