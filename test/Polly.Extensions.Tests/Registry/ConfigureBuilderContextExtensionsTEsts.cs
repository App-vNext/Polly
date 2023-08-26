using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Polly.Registry;

namespace Polly.Extensions.Tests.Registry;

public class ConfigureBuilderContextExtensionsTests
{
    [Fact]
    public void ConfigureReloads_MonitorRegistrationReturnsNull_DoesNotThrow()
    {
        var monitor = Substitute.For<IOptionsMonitor<Options>>();
        monitor.OnChange(default!).ReturnsNullForAnyArgs();

        var listener = new FakeTelemetryListener();
        var registry = new ResiliencePipelineRegistry<string>();
        var pipeline = registry.GetOrAddPipeline("pipeline", (builder, context) =>
        {
            builder.TelemetryListener = listener;
            builder.AddConcurrencyLimiter(1);
            context.EnableReloads(monitor);
        });

        registry.Dispose();

        listener.Events.Should().BeEmpty();
    }

    public class Options
    {
    }
}
