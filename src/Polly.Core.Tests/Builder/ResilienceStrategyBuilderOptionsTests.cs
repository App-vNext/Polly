using Moq;
using Polly.Builder;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new ResilienceStrategyBuilderOptions();

        options.BuilderName.Should().Be("");
        options.Properties.Should().NotBeNull();
        options.TimeProvider.Should().Be(TimeProvider.System);
        options.TelemetryFactory.Should().Be(NullResilienceTelemetryFactory.Instance);
    }

    [Fact]
    public void Ctor_Copy_EnsureCopied()
    {
        var options = new ResilienceStrategyBuilderOptions
        {
            BuilderName = "test",
            TelemetryFactory = Mock.Of<ResilienceTelemetryFactory>(),
            TimeProvider = new FakeTimeProvider().Object
        };

        options.Properties.Set(new ResiliencePropertyKey<int>("A"), 1);
        options.Properties.Set(new ResiliencePropertyKey<int>("B"), 2);

        var other = new ResilienceStrategyBuilderOptions(options);

        other.BuilderName.Should().Be("test");
        other.TelemetryFactory.Should().BeSameAs(options.TelemetryFactory);
        other.TimeProvider.Should().BeSameAs(options.TimeProvider);
        other.Properties.Should().BeEquivalentTo(options.Properties);
    }
}
