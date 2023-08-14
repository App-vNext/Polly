using Microsoft.Extensions.Time.Testing;
using Polly.Utils;

namespace Polly.Core.Tests;

public class GenericResiliencePipelineBuilderTests
{
    private readonly ResiliencePipelineBuilder<string> _builder = new();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        _builder.Name.Should().BeNull();
        _builder.TimeProvider.Should().Be(TimeProvider.System);
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        new ResiliencePipelineBuilder<string>(new ResiliencePipelineBuilder()).Should().NotBeNull();
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
        strategy.Strategy.Should().BeOfType<CompositeResiliencePipeline>().Subject.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void AddGenericStrategy_Ok()
    {
        // arrange
        var testStrategy = new ResiliencePipeline<string>(new TestResilienceStrategy().AsPipeline());
        _builder.AddPipeline(testStrategy);

        // act
        var strategy = _builder.Build();

        // assert
        strategy.Should().NotBeNull();
        ((CompositeResiliencePipeline)strategy.Strategy).Strategies[0].Should().Be(testStrategy.Strategy);
    }
}
