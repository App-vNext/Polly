using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Testing;
using Polly.Utils;

namespace Polly.Core.Tests;

public class ResiliencePipelineBuilderTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Name.Should().BeNull();
        builder.TimeProvider.Should().Be(TimeProvider.System);
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        var builder = new ResiliencePipelineBuilder
        {
            TimeProvider = Substitute.For<TimeProvider>(),
            Name = "dummy",
            TelemetryListener = Substitute.For<TelemetryListener>(),
        };

        var other = new ResiliencePipelineBuilder<double>(builder);
        other.Name.Should().Be(builder.Name);
        other.TimeProvider.Should().Be(builder.TimeProvider);
        other.TelemetryListener.Should().BeSameAs(builder.TelemetryListener);
    }

    [Fact]
    public void AddPipeline_Single_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResiliencePipelineBuilder();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(3),
        };

        builder.AddPipeline(first.AsPipeline());

        // act
        var pipeline = builder.Build();

        // assert
        pipeline.Execute(_ => executions.Add(2));

        pipeline.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<TestResilienceStrategy>();
        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(3);
    }

    [Fact]
    public void AddPipeline_Multiple_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResiliencePipelineBuilder();
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

        builder.AddPipeline(first.AsPipeline());
        builder.AddPipeline(second.AsPipeline());
        builder.AddPipeline(third.AsPipeline());

        // act
        var strategy = builder.Build();
        strategy
            .Should()
            .BeOfType<CompositeResiliencePipeline>()
            .Subject
            .Strategies.Should().HaveCount(3);

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void AddPipeline_Duplicate_Throws()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResiliencePipelineBuilder()
            .AddPipeline(NullResiliencePipeline.Instance)
            .AddPipeline(NullResiliencePipeline.Instance);

        builder.Invoking(b => b.Build())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("The composite resilience strategy must contain unique resilience strategies.");
    }

    [Fact]
    public void Validator_Ok()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Validator.Should().NotBeNull();

        builder.Validator(new ResilienceValidationContext("ABC", "ABC"));

        builder
            .Invoking(b => b.Validator(new ResilienceValidationContext(new RetryStrategyOptions { RetryCount = -4 }, "The primary message.")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The primary message.
            Validation Errors:
            The field RetryCount must be between 1 and 2147483647.
            """);
    }

    [Fact]
    public void AddPipeline_MultipleNonDelegating_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new ResiliencePipelineBuilder();
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

        builder.AddPipeline(first.AsPipeline());
        builder.AddPipeline(second.AsPipeline());
        builder.AddPipeline(third.AsPipeline());

        // act
        var strategy = builder.Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void Build_Empty_ReturnsNullResiliencePipeline() => new ResiliencePipelineBuilder().Build().Should().BeSameAs(NullResiliencePipeline.Instance);

    [Fact]
    public void AddPipeline_AfterUsed_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Build();

        builder
            .Invoking(b => b.AddPipeline(NullResiliencePipeline.Instance))
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot add any more resilience strategies to the builder after it has been used to build a pipeline once.");
    }

    [Fact]
    public void Build_InvalidBuilderOptions_Throw()
    {
        var builder = new InvalidResiliencePipelineBuilder();

        builder.Invoking(b => b.BuildPipeline())
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'ResiliencePipelineBuilder' configuration is invalid.

Validation Errors:
The RequiredProperty field is required.
""");
    }

    [Fact]
    public void AddPipeline_InvalidOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        builder
            .Invoking(b => b.AddStrategy(_ => new TestResilienceStrategy(), new InvalidResiliencePipelineOptions()))
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'InvalidResiliencePipelineOptions' are invalid.

Validation Errors:
The RequiredProperty field is required.
""");
    }

    [Fact]
    public void AddPipeline_NullFactory_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        builder
            .Invoking(b => b.AddStrategy((Func<StrategyBuilderContext, ResilienceStrategy>)null!, new TestResilienceStrategyOptions()))
            .Should()
            .Throw<ArgumentNullException>()
            .And.ParamName
            .Should()
            .Be("factory");

        builder
            .Invoking(b => b.AddStrategy((Func<StrategyBuilderContext, ResilienceStrategy<object>>)null!, new TestResilienceStrategyOptions()))
            .Should()
            .Throw<ArgumentNullException>()
            .And.ParamName
            .Should()
            .Be("factory");
    }

    [Fact]
    public void AddPipeline_CombinePipelines_Ok()
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

        var pipeline1 = new ResiliencePipelineBuilder().AddPipeline(first.AsPipeline()).AddPipeline(second.AsPipeline()).Build();

        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(3),
            After = (_, _) => executions.Add(5),
        };
        var pipeline2 = new ResiliencePipelineBuilder().AddPipeline(third.AsPipeline()).Build();

        // act
        var strategy = new ResiliencePipelineBuilder().AddPipeline(pipeline1).AddPipeline(pipeline2).Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void Build_EnsureCorrectContext()
    {
        // arrange
        var verified1 = false;
        var verified2 = false;

        var builder = new ResiliencePipelineBuilder
        {
            Name = "builder-name",
            TimeProvider = new FakeTimeProvider(),
        };

        builder.AddStrategy(
            context =>
            {
                context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
                context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
                context.Telemetry.Should().NotBeNull();
                context.TimeProvider.Should().Be(builder.TimeProvider);
                verified1 = true;

                return new TestResilienceStrategy();
            },
            new TestResilienceStrategyOptions { Name = "strategy-name" });

        builder.AddStrategy(
            context =>
            {
                context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
                context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name-2");
                context.Telemetry.Should().NotBeNull();
                context.TimeProvider.Should().Be(builder.TimeProvider);
                verified2 = true;

                return new TestResilienceStrategy();
            },
            new TestResilienceStrategyOptions { Name = "strategy-name-2" });

        // act
        builder.Build();

        // assert
        verified1.Should().BeTrue();
        verified2.Should().BeTrue();
    }

    [Fact]
    public void EmptyOptions_Ok() => ResiliencePipelineBuilderExtensions.EmptyOptions.Instance.Name.Should().BeNull();

    [Fact]
    public void ExecuteAsync_EnsureReceivedCallbackExecutesNextStrategy()
    {
        // arrange
        var executions = new List<string>();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add("first-start"),
            After = (_, _) => executions.Add("first-end"),
        }.AsPipeline();

        var second = new ExecuteCallbackTwiceStrategy
        {
            Before = () => executions.Add("second-start"),
            After = () => executions.Add("second-end"),
        }.AsPipeline();

        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add("third-start"),
            After = (_, _) => executions.Add("third-end"),
        }.AsPipeline();

        var strategy = new ResiliencePipelineBuilder().AddPipeline(first).AddPipeline(second).AddPipeline(third).Build();

        // act
        strategy.Execute(_ => executions.Add("execute"));

        // assert
        executions.SequenceEqual(new[]
        {
            "first-start",
            "second-start",
            "third-start",
            "execute",
            "third-end",
            "third-start",
            "execute",
            "third-end",
            "second-end",
            "first-end",
        })
        .Should()
        .BeTrue();
    }

    private class Strategy : ResilienceStrategy
    {
        public Action? Before { get; set; }

        public Action? After { get; set; }

        protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
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

    private class ExecuteCallbackTwiceStrategy : ResilienceStrategy
    {
        public Action? Before { get; set; }

        public Action? After { get; set; }

        protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            try
            {
                Before?.Invoke();
                await callback(context, state);
                return await callback(context, state);
            }
            finally
            {
                After?.Invoke();
            }
        }
    }

    private class InvalidResiliencePipelineOptions : ResilienceStrategyOptions
    {
        [Required]
        public string? RequiredProperty { get; set; }
    }

    private class InvalidResiliencePipelineBuilder : ResiliencePipelineBuilderBase
    {
        [Required]
        public string? RequiredProperty { get; set; }
    }
}
