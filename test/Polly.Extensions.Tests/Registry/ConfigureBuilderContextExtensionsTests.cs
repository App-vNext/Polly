using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Polly.Registry;

namespace Polly.Extensions.Tests.Registry;

public class ConfigureBuilderContextExtensionsTests
{
    [Fact]
    public async Task ConfigureReloads_NullArguments_Throws()
    {
        await using var registry = new ResiliencePipelineRegistry<string>();
        registry.GetOrAddPipeline("pipeline", (_, builderContext) =>
        {
            ConfigureBuilderContext<string> nullContext = null!;
            IOptionsMonitor<Options> nullMonitor = null!;

            var monitor = Substitute.For<IOptionsMonitor<Options>>();

            Assert.Throws<ArgumentNullException>("context", () => nullContext.EnableReloads(monitor));
            Assert.Throws<ArgumentNullException>("optionsMonitor", () => builderContext.EnableReloads(nullMonitor));
        });
    }

    [Fact]
    public async Task ConfigureReloads_MonitorRegistrationReturnsNull_DoesNotThrow()
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

        await registry.DisposeAsync();

        listener.Events.ShouldBeEmpty();
    }

#pragma warning disable S2094 // Classes should not be empty
    public class Options;
#pragma warning restore S2094 // Classes should not be empty
}
