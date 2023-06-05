using FluentAssertions;
using Polly.Strategy;
using Polly.Utils;

namespace Polly.Core.Tests;

public class GenericResilienceStrategyBuilderTests
{
    private readonly ResilienceStrategyBuilder<string> _builder = new();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        _builder.BuilderName.Should().BeNull();
        _builder.Properties.Should().NotBeNull();
        _builder.TimeProvider.Should().Be(TimeProvider.System);
        _builder.OnCreatingStrategy.Should().BeNull();
        _builder.IsGenericBuilder.Should().BeTrue();
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        new ResilienceStrategyBuilder<string>(new ResilienceStrategyBuilder()).IsGenericBuilder.Should().BeTrue();
    }

    [Fact]
    public void Properties_GetSet_Ok()
    {
        _builder.BuilderName = "dummy";
        _builder.BuilderName.Should().Be("dummy");

        var timeProvider = new FakeTimeProvider().Object;
        _builder.TimeProvider = timeProvider;
        _builder.TimeProvider.Should().Be(timeProvider);

        _builder.OnCreatingStrategy = s => { };
        _builder.OnCreatingStrategy.Should().NotBeNull();
    }

    [Fact]
    public void Build_Ok()
    {
        // arrange
        _builder.AddStrategy(new TestResilienceStrategy());
        _builder.AddStrategy(_ => new TestResilienceStrategy(), new TestResilienceStrategyOptions());

        // act
        var strategy = _builder.Build();

        // assert
        strategy.Should().NotBeNull();
        strategy.Strategy.Should().BeOfType<ResilienceStrategyPipeline>().Subject.Strategies.Should().HaveCount(2);
    }
}
