using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Testing;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public class GenericResiliencePipelineBuilderTests
{
    private readonly ResiliencePipelineBuilder<string> _builder = new();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        _builder.Name.ShouldBeNull();
        _builder.TimeProvider.ShouldBeNull();
    }

    [Fact]
    public void CopyCtor_Ok() =>
        new ResiliencePipelineBuilder<string>(new ResiliencePipelineBuilder()).ShouldNotBeNull();

    [Fact]
    public void Properties_GetSet_Ok()
    {
        _builder.Name = "dummy";
        _builder.Name.ShouldBe("dummy");

        var timeProvider = new FakeTimeProvider();
        _builder.TimeProvider = timeProvider;
        _builder.TimeProvider.ShouldBe(timeProvider);
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
        strategy.ShouldNotBeNull();
        strategy.Component.ShouldBeOfType<CompositeComponent>().Components.Count.ShouldBe(2);
    }

    [Fact]
    public void AddGenericStrategy_Ok()
    {
        // arrange
        var strategy = Substitute.For<ResilienceStrategy<string>>();
        _builder.AddStrategy(strategy);

        // act
        var pipeline = _builder.Build();

        // assert
        strategy.ShouldNotBeNull();
        ((CompositeComponent)pipeline.Component).Components[0]
            .ShouldBeOfType<BridgeComponent<string>>().Strategy
            .ShouldBe(strategy);
    }

    [Fact]
    public void AddStrategy_ExplicitInstance_Ok()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        var strategy = Substitute.For<ResilienceStrategy<string>>();

        builder.AddStrategy(_ => strategy);

        builder
            .Build()
            .GetPipelineDescriptor()
            .FirstStrategy.StrategyInstance
            .ShouldBeSameAs(strategy);
    }
}
