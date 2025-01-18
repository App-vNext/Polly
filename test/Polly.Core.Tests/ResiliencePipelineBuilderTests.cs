using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Testing;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public class ResiliencePipelineBuilderTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Name.ShouldBeNull();
        builder.TimeProvider.ShouldBeNull();
    }

    [Fact]
    public void TimeProviderInternal_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        builder.TimeProviderInternal.ShouldBe(TimeProvider.System);

        var timeProvider = Substitute.For<TimeProvider>();
        builder.TimeProvider = timeProvider;

        builder.TimeProvider.ShouldBe(timeProvider);
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
        other.Name.ShouldBe(builder.Name);
        other.TimeProvider.ShouldBe(builder.TimeProvider);
        other.TelemetryListener.ShouldBeSameAs(builder.TelemetryListener);
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

        builder.AddStrategy(first);

        // act
        var pipeline = builder.Build();

        // assert
        pipeline.Execute(_ => executions.Add(2));

        pipeline.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<TestResilienceStrategy>();
        executions.OrderBy(p => p).ShouldBe(executions);
        executions.Count.ShouldBe(3);
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
            .Component
            .ShouldBeOfType<CompositeComponent>()
            .Components
            .Count
            .ShouldBe(3);

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.OrderBy(p => p).ShouldBe(executions);
        executions.Count.ShouldBe(7);
    }

    [Fact]
    public void AddStrategy_ExplicitProactiveInstance_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        var strategy = new TestResilienceStrategy();

        builder.AddStrategy(_ => strategy);

        builder
            .Build()
            .GetPipelineDescriptor()
            .FirstStrategy.StrategyInstance
            .ShouldBeSameAs(strategy);
    }

    [Fact]
    public void AddStrategy_ExplicitReactiveInstance_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        var strategy = Substitute.For<ResilienceStrategy<object>>();

        builder.AddStrategy(_ => strategy);

        builder
            .Build()
            .GetPipelineDescriptor()
            .FirstStrategy.StrategyInstance
            .ShouldBeSameAs(strategy);
    }

    [Fact]
    public void Validator_Ok()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Validator.ShouldNotBeNull();

        builder.Validator(new ResilienceValidationContext("ABC", "ABC"));

        var exception = Should.Throw<ValidationException>(
            () => builder.Validator(new ResilienceValidationContext(new RetryStrategyOptions { MaxRetryAttempts = -4 }, "The primary message.")));

        exception.Message.Trim().ShouldBe("""
            The primary message.
            Validation Errors:
            The field MaxRetryAttempts must be between 1 and 2147483647.
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

        executions.OrderBy(p => p).ShouldBe(executions);
        executions.Count.ShouldBe(7);
    }

    [Fact]
    public void Build_Empty_ReturnsNullResiliencePipeline() => new ResiliencePipelineBuilder().Build().Component.ShouldBeSameAs(PipelineComponent.Empty);

    [Fact]
    public void AddPipeline_AfterUsed_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Build();

        var exception = Should.Throw<InvalidOperationException>(() => builder.AddPipeline(ResiliencePipeline.Empty));
        exception.Message.ShouldBe("Cannot add any more resilience strategies to the builder after it has been used to build a pipeline once.");
    }

    [Fact]
    public void Build_InvalidBuilderOptions_Throw()
    {
        var builder = new InvalidResiliencePipelineBuilder();

        var exception = Should.Throw<ValidationException>(builder.BuildPipelineComponent);
        exception.Message.Trim().ShouldBe(
            """
            The 'ResiliencePipelineBuilder' configuration is invalid.
            Validation Errors:
            The RequiredProperty field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void AddPipeline_InvalidOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        var exception = Should.Throw<ValidationException>(() => builder.AddStrategy(_ => new TestResilienceStrategy(), new InvalidResiliencePipelineOptions()));
        exception.Message.Trim().ShouldBe(
            """
            The 'InvalidResiliencePipelineOptions' are invalid.
            Validation Errors:
            The RequiredProperty field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void AddPipeline_NullFactory_Throws()
    {
        var builder = new ResiliencePipelineBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.AddStrategy((Func<StrategyBuilderContext, ResilienceStrategy>)null!, new TestResilienceStrategyOptions()))
            .ParamName.ShouldBe("factory");

        Should.Throw<ArgumentNullException>(
            () => builder.AddStrategy((Func<StrategyBuilderContext, ResilienceStrategy<object>>)null!, new TestResilienceStrategyOptions()))
            .ParamName.ShouldBe("factory");
    }

    [Fact]
    public async Task AddPipeline_EnsureNotDisposed()
    {
        var externalComponent = Substitute.For<PipelineComponent>();
        var externalBuilder = new ResiliencePipelineBuilder();
        externalBuilder.AddPipelineComponent(_ => externalComponent, new TestResilienceStrategyOptions());
        var externalPipeline = externalBuilder.Build();

        var internalComponent = Substitute.For<PipelineComponent>();
        var builder = new ResiliencePipelineBuilder();
        builder
            .AddPipeline(externalPipeline)
            .AddPipelineComponent(_ => internalComponent, new TestResilienceStrategyOptions());
        var pipeline = builder.Build();

        await pipeline.DisposeHelper.DisposeAsync();
        await externalComponent.Received(0).DisposeAsync();
        await internalComponent.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task AddPipeline_Generic_EnsureNotDisposed()
    {
        var externalComponent = Substitute.For<PipelineComponent>();
        var externalBuilder = new ResiliencePipelineBuilder<string>();
        externalBuilder.AddPipelineComponent(_ => externalComponent, new TestResilienceStrategyOptions());
        var externalPipeline = externalBuilder.Build();

        var internalComponent = Substitute.For<PipelineComponent>();
        var builder = new ResiliencePipelineBuilder<string>();
        builder
            .AddPipeline(externalPipeline)
            .AddPipelineComponent(_ => internalComponent, new TestResilienceStrategyOptions());
        var pipeline = builder.Build();

        pipeline.Execute(_ => string.Empty);

        await pipeline.DisposeHelper.DisposeAsync();
        await externalComponent.Received(0).DisposeAsync();
        await internalComponent.Received(1).DisposeAsync();
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

        executions.OrderBy(p => p).ShouldBe(executions);
        executions.Count.ShouldBe(7);
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
                context.Telemetry.TelemetrySource.PipelineName.ShouldBe("builder-name");
                context.Telemetry.TelemetrySource.StrategyName.ShouldBe("strategy_name");
                context.Telemetry.ShouldNotBeNull();
                context.TimeProvider.ShouldBe(builder.TimeProvider);
                verified1 = true;

                return new TestResilienceStrategy();
            },
            new TestResilienceStrategyOptions { Name = "strategy_name" });

        builder.AddStrategy(
            context =>
            {
                context.Telemetry.TelemetrySource.PipelineName.ShouldBe("builder-name");
                context.Telemetry.TelemetrySource.StrategyName.ShouldBe("strategy_name-2");
                context.Telemetry.ShouldNotBeNull();
                context.TimeProvider.ShouldBe(builder.TimeProvider);
                verified2 = true;

                return new TestResilienceStrategy();
            },
            new TestResilienceStrategyOptions { Name = "strategy_name-2" });

        // act
        builder.Build();

        // assert
        verified1.ShouldBeTrue();
        verified2.ShouldBeTrue();
    }

    [Fact]
    public void EmptyOptions_Ok() => ResiliencePipelineBuilderExtensions.EmptyOptions.Instance.Name.ShouldBeNull();

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
        .ShouldBeTrue();
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
