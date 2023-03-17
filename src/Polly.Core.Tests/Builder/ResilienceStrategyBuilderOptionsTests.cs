using FluentAssertions;
using Polly.Builder;
using Xunit;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new ResilienceStrategyBuilderOptions();

        options.BuilderName.Should().Be("");
    }
}
