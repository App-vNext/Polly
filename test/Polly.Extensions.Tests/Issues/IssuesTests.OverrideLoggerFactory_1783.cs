using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void OverrideLoggerFactory_ByExplicitConfigureTelemetryCall_1783()
    {
        // Arrange
        using var loggerFactory1 = new FakeLoggerFactory();
        using var loggerFactory2 = new FakeLoggerFactory();
        var services = new ServiceCollection();
        services.TryAddSingleton<ILoggerFactory>(loggerFactory1);

        // Act
        services.AddResiliencePipeline("dummy", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1));

            // This call should override the base factory
            builder.ConfigureTelemetry(loggerFactory2);
        });

        // Assert
        var provider = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();

        provider.GetPipeline("dummy").Execute(() => { });

        loggerFactory1.FakeLogger.GetRecords().ShouldBeEmpty();
        loggerFactory2.FakeLogger.GetRecords().ShouldNotBeEmpty();
    }
}
