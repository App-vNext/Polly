using System;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Moq;
using Polly.Builder;
using Polly.Core.Tests.Utils;
using Polly.Telemetry;
using Xunit;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderTests
{
    [Fact]
    public void AddStrategy_Single_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResilienceStrategyBuilder();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(3),
        };

        builder.AddStrategy(first);

        // act
        var strategy = builder.Build();

        // assert
        strategy.Execute(_ => executions.Add(2));
        strategy.Should().BeOfType<TestResilienceStrategy>();
        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(3);
    }

    [Fact]
    public void AddStrategy_Multiple_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResilienceStrategyBuilder();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(7),
        };
        var second = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(2),
            After = (_, _) => executions.Add(6),
        };
        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(3),
            After = (_, _) => executions.Add(5),
        };

        builder.AddStrategy(first);
        builder.AddStrategy(second);
        builder.AddStrategy(third);

        // act
        var strategy = builder.Build();
        strategy
            .Should()
            .BeOfType<ResilienceStrategyPipeline>()
            .Subject
            .Strategies.Should().HaveCount(3);

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void AddStrategy_Duplicate_Throws()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResilienceStrategyBuilder()
            .AddStrategy(NullResilienceStrategy.Instance)
            .AddStrategy(NullResilienceStrategy.Instance);

        builder.Invoking(b => b.Build())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("The resilience pipeline must contain unique resilience strategies.");
    }

    [Fact]
    public void AddStrategy_MultipleNonDelegating_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResilienceStrategyBuilder();
        var first = new Strategy
        {
            Before = () => executions.Add(1),
            After = () => executions.Add(7),
        };
        var second = new Strategy
        {
            Before = () => executions.Add(2),
            After = () => executions.Add(6),
        };
        var third = new Strategy
        {
            Before = () => executions.Add(3),
            After = () => executions.Add(5),
        };

        builder.AddStrategy(first);
        builder.AddStrategy(second);
        builder.AddStrategy(third);

        // act
        var strategy = builder.Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void Build_Empty_ReturnsNullResilienceStrategy()
    {
        new ResilienceStrategyBuilder().Build().Should().BeSameAs(NullResilienceStrategy.Instance);
    }

    [Fact]
    public void AddStrategy_AfterUsed_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder.Build();

        builder
            .Invoking(b => b.AddStrategy(NullResilienceStrategy.Instance))
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot add any more resilience strategies to the builder after it has been used to build a strategy once.");
    }

    [Fact]
    public void Options_SetNull_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder.Invoking(b => b.Options = null!).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_InvalidBuilderOptions_Throw()
    {
        var builder = new ResilienceStrategyBuilder();
        builder.Options.BuilderName = null!;

        builder.Invoking(b => b.Build())
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'ResilienceStrategyBuilderOptions' options are not valid.

Validation Errors:
The BuilderName field is required.
""");
    }

    [Fact]
    public void AddStrategy_InvalidOptions_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddStrategy(NullResilienceStrategy.Instance, new ResilienceStrategyOptions { StrategyName = null!, StrategyType = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'ResilienceStrategyOptions' options are not valid.

Validation Errors:
The StrategyName field is required.
The StrategyType field is required.
""");
    }

    [Fact]
    public void AddStrategy_NullFactory_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddStrategy((Func<ResilienceStrategyBuilderContext, ResilienceStrategy>)null!))
            .Should()
            .Throw<ArgumentNullException>()
            .And.ParamName
            .Should()
            .Be("factory");
    }

    [Fact]
    public void AddStrategy_CombinePipelines_Ok()
    {
        // arrange
        var executions = new List<int>();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(7),
        };
        var second = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(2),
            After = (_, _) => executions.Add(6),
        };

        var pipeline1 = new ResilienceStrategyBuilder().AddStrategy(first).AddStrategy(second).Build();

        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(3),
            After = (_, _) => executions.Add(5),
        };
        var pipeline2 = new ResilienceStrategyBuilder().AddStrategy(third).Build();

        // act
        var strategy = new ResilienceStrategyBuilder().AddStrategy(pipeline1).AddStrategy(pipeline2).Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void BuildStrategy_EnsureCorrectContext()
    {
        // arrange
        bool verified1 = false;
        bool verified2 = false;

        var builder = new ResilienceStrategyBuilder
        {
            Options = new ResilienceStrategyBuilderOptions
            {
                BuilderName = "builder-name",
                TimeProvider = new FakeTimeProvider().Object
            }
        };

        builder.AddStrategy(
            context =>
            {
                context.BuilderName.Should().Be("builder-name");
                context.StrategyName.Should().Be("strategy-name");
                context.StrategyType.Should().Be("strategy-type");
                context.BuilderProperties.Should().BeSameAs(builder.Options.Properties);
                context.Telemetry.Should().NotBeNull();
                context.Telemetry.Should().Be(NullResilienceTelemetry.Instance);
                context.TimeProvider.Should().Be(builder.Options.TimeProvider);
                verified1 = true;

                return new TestResilienceStrategy();
            },
            new ResilienceStrategyOptions { StrategyName = "strategy-name", StrategyType = "strategy-type" });

        builder.AddStrategy(
            context =>
            {
                context.BuilderName.Should().Be("builder-name");
                context.StrategyName.Should().Be("strategy-name-2");
                context.StrategyType.Should().Be("strategy-type-2");
                context.BuilderProperties.Should().BeSameAs(builder.Options.Properties);
                context.Telemetry.Should().NotBeNull();
                context.Telemetry.Should().Be(NullResilienceTelemetry.Instance);
                context.TimeProvider.Should().Be(builder.Options.TimeProvider);
                verified2 = true;

                return new TestResilienceStrategy();
            },
            new ResilienceStrategyOptions { StrategyName = "strategy-name-2", StrategyType = "strategy-type-2" });

        // act
        builder.Build();

        // assert
        verified1.Should().BeTrue();
        verified2.Should().BeTrue();
    }

    [Fact]
    public void BuildStrategy_EnsureTelemetryFactoryInvoked()
    {
        // arrange
        var factory = new Mock<ResilienceTelemetryFactory>(MockBehavior.Strict);
        var builder = new ResilienceStrategyBuilder
        {
            Options = new ResilienceStrategyBuilderOptions
            {
                BuilderName = "builder-name",
                TelemetryFactory = factory.Object
            },
        };

        factory
            .Setup(v => v.Create(It.IsAny<ResilienceTelemetryFactoryContext>()))
            .Returns(NullResilienceTelemetry.Instance)
            .Callback<ResilienceTelemetryFactoryContext>(context =>
            {
                context.BuilderName.Should().Be("builder-name");
                context.StrategyName.Should().Be("strategy-name");
                context.StrategyType.Should().Be("strategy-type");
                context.BuilderProperties.Should().BeSameAs(builder.Options.Properties);
            });

        builder.AddStrategy(
            context =>
            {
                return new TestResilienceStrategy();
            },
            new ResilienceStrategyOptions { StrategyName = "strategy-name", StrategyType = "strategy-type" });

        // act
        builder.Build();

        // assert
        factory.VerifyAll();
    }

    private class Strategy : ResilienceStrategy
    {
        public Action? Before { get; set; }

        public Action? After { get; set; }

        protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
        {
            try
            {
                Before?.Invoke();
                return await callback(context, state);
            }
            finally
            {
                After?.Invoke();
            }
        }
    }
}
