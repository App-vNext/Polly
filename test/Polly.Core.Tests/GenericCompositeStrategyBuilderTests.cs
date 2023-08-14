using Microsoft.Extensions.Time.Testing;
using Polly.Utils;

namespace Polly.Core.Tests;

public class GenericCompositeStrategyBuilderTests
{
    private readonly CompositeStrategyBuilder<string> _builder = new();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        _builder.Name.Should().BeNull();
        _builder.TimeProvider.Should().Be(TimeProvider.System);
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        new CompositeStrategyBuilder<string>(new CompositeStrategyBuilder()).Should().NotBeNull();
    }

    [Fact]
    public void Properties_GetSet_Ok()
    {
        _builder.Name = "dummy";
        _builder.Name.Should().Be("dummy");

        var timeProvider = new FakeTimeProvider();
        _builder.TimeProvider = timeProvider;
        _builder.TimeProvider.Should().Be(timeProvider);
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
        strategy.Strategy.Should().BeOfType<CompositeResilienceStrategy>().Subject.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void AddGenericStrategy_Ok()
    {
        // arrange
        var testStrategy = new ResilienceStrategy<string>(new TestResilienceStrategy().AsStrategy());
        _builder.AddStrategy(testStrategy);

        // act
        var strategy = _builder.Build();

        // assert
        strategy.Should().NotBeNull();
        ((CompositeResilienceStrategy)strategy.Strategy).Strategies[0].Should().Be(testStrategy.Strategy);
    }
}
