using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Retry;
using Polly.Utils;

namespace Polly.Core.Tests;

public class CompositeStrategyBuilderTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var builder = new CompositeStrategyBuilder();

        builder.Name.Should().BeNull();
        builder.Properties.Should().NotBeNull();
        builder.TimeProvider.Should().Be(TimeProvider.System);
        builder.Randomizer.Should().NotBeNull();
    }

    [Fact]
    public void CopyCtor_Ok()
    {
        var builder = new CompositeStrategyBuilder
        {
            TimeProvider = Substitute.For<TimeProvider>(),
            Name = "dummy",
            Randomizer = () => 0.0,
            DiagnosticSource = Substitute.For<DiagnosticSource>(),
            OnCreatingStrategy = _ => { },
        };

        builder.Properties.Set(new ResiliencePropertyKey<string>("dummy"), "dummy");

        var other = new CompositeStrategyBuilder<double>(builder);
        other.Name.Should().Be(builder.Name);
        other.TimeProvider.Should().Be(builder.TimeProvider);
        other.Randomizer.Should().BeSameAs(builder.Randomizer);
        other.DiagnosticSource.Should().BeSameAs(builder.DiagnosticSource);
        other.OnCreatingStrategy.Should().BeSameAs(builder.OnCreatingStrategy);
        other.Properties.GetValue(new ResiliencePropertyKey<string>("dummy"), "").Should().Be("dummy");
    }

    [Fact]
    public void AddStrategy_Single_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new CompositeStrategyBuilder();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(3),
        };

        builder.AddStrategy(first.AsStrategy());

        // act
        var strategy = builder.Build();

        // assert
        strategy.Execute(_ => executions.Add(2));
        ((NonReactiveResilienceStrategyBridge)strategy).Strategy.Should().BeOfType<TestResilienceStrategy>();
        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(3);
    }

    [Fact]
    public void AddStrategy_Multiple_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new CompositeStrategyBuilder();
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

        builder.AddStrategy(first.AsStrategy());
        builder.AddStrategy(second.AsStrategy());
        builder.AddStrategy(third.AsStrategy());

        // act
        var strategy = builder.Build();
        strategy
            .Should()
            .BeOfType<CompositeResilienceStrategy>()
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
        var builder = new CompositeStrategyBuilder()
            .AddStrategy(NullResilienceStrategy.Instance)
            .AddStrategy(NullResilienceStrategy.Instance);

        builder.Invoking(b => b.Build())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("The composite resilience strategy must contain unique resilience strategies.");
    }

    [Fact]
    public void Validator_Ok()
    {
        var builder = new CompositeStrategyBuilder();

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
    public void AddStrategy_MultipleNonDelegating_Ok()
    {
        // arrange
        var executions = new List<int>();
        var builder = new CompositeStrategyBuilder();
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

        builder.AddStrategy(first.AsStrategy());
        builder.AddStrategy(second.AsStrategy());
        builder.AddStrategy(third.AsStrategy());

        // act
        var strategy = builder.Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void Build_Empty_ReturnsNullResilienceStrategy() => new CompositeStrategyBuilder().Build().Should().BeSameAs(NullResilienceStrategy.Instance);

    [Fact]
    public void AddStrategy_AfterUsed_Throws()
    {
        var builder = new CompositeStrategyBuilder();

        builder.Build();

        builder
            .Invoking(b => b.AddStrategy(NullResilienceStrategy.Instance))
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot add any more resilience strategies to the builder after it has been used to build a strategy once.");
    }

    [Fact]
    public void Build_InvalidBuilderOptions_Throw()
    {
        var builder = new InvalidCompositeStrategyBuilder();

        builder.Invoking(b => b.BuildStrategy())
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'CompositeStrategyBuilder' configuration is invalid.

Validation Errors:
The RequiredProperty field is required.
""");
    }

    [Fact]
    public void AddStrategy_InvalidOptions_Throws()
    {
        var builder = new CompositeStrategyBuilder();

        builder
            .Invoking(b => b.AddStrategy(_ => new TestResilienceStrategy(), new InvalidResilienceStrategyOptions()))
            .Should()
            .Throw<ValidationException>()
            .WithMessage(
"""
The 'InvalidResilienceStrategyOptions' are invalid.

Validation Errors:
The RequiredProperty field is required.
""");
    }

    [Fact]
    public void AddStrategy_NullFactory_Throws()
    {
        var builder = new CompositeStrategyBuilder();

        builder
            .Invoking(b => b.AddStrategy((Func<StrategyBuilderContext, NonReactiveResilienceStrategy>)null!, new TestResilienceStrategyOptions()))
            .Should()
            .Throw<ArgumentNullException>()
            .And.ParamName
            .Should()
            .Be("factory");

        builder
            .Invoking(b => b.AddStrategy((Func<StrategyBuilderContext, ReactiveResilienceStrategy<object>>)null!, new TestResilienceStrategyOptions()))
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

        var pipeline1 = new CompositeStrategyBuilder().AddStrategy(first.AsStrategy()).AddStrategy(second.AsStrategy()).Build();

        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(3),
            After = (_, _) => executions.Add(5),
        };
        var pipeline2 = new CompositeStrategyBuilder().AddStrategy(third.AsStrategy()).Build();

        // act
        var strategy = new CompositeStrategyBuilder().AddStrategy(pipeline1).AddStrategy(pipeline2).Build();

        // assert
        strategy.Execute(_ => executions.Add(4));

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(7);
    }

    [Fact]
    public void BuildStrategy_EnsureCorrectContext()
    {
        // arrange
        var verified1 = false;
        var verified2 = false;

        var builder = new CompositeStrategyBuilder
        {
            Name = "builder-name",
            TimeProvider = new FakeTimeProvider(),
        };

        builder.AddStrategy(
            context =>
            {
                context.BuilderName.Should().Be("builder-name");
                context.StrategyName.Should().Be("strategy-name");
                context.BuilderProperties.Should().BeSameAs(builder.Properties);
                context.Telemetry.Should().NotBeNull();
                context.TimeProvider.Should().Be(builder.TimeProvider);
                context.Randomizer.Should().BeSameAs(builder.Randomizer);
                verified1 = true;

                return new TestResilienceStrategy();
            },
            new TestResilienceStrategyOptions { Name = "strategy-name" });

        builder.AddStrategy(
            context =>
            {
                context.BuilderName.Should().Be("builder-name");
                context.StrategyName.Should().Be("strategy-name-2");
                context.BuilderProperties.Should().BeSameAs(builder.Properties);
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
    public void Build_OnCreatingStrategy_EnsureRespected()
    {
        // arrange
        var strategy = new TestResilienceStrategy().AsStrategy();
        var builder = new CompositeStrategyBuilder
        {
            OnCreatingStrategy = strategies =>
            {
                strategies.Should().ContainSingle(s => s == strategy);
                strategies.Insert(0, new TestResilienceStrategy().AsStrategy());
            }
        };

        builder.AddStrategy(strategy);

        // act
        var finalStrategy = builder.Build();

        // assert
        finalStrategy.Should().BeOfType<CompositeResilienceStrategy>();
    }

    [Fact]
    public void EmptyOptions_Ok() => CompositeStrategyBuilderExtensions.EmptyOptions.Instance.Name.Should().BeNull();

    [Fact]
    public void ExecuteAsync_EnsureReceivedCallbackExecutesNextStrategy()
    {
        // arrange
        var executions = new List<string>();
        var first = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add("first-start"),
            After = (_, _) => executions.Add("first-end"),
        }.AsStrategy();

        var second = new ExecuteCallbackTwiceStrategy
        {
            Before = () => executions.Add("second-start"),
            After = () => executions.Add("second-end"),
        }.AsStrategy();

        var third = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add("third-start"),
            After = (_, _) => executions.Add("third-end"),
        }.AsStrategy();

        var strategy = new CompositeStrategyBuilder().AddStrategy(first).AddStrategy(second).AddStrategy(third).Build();

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

    private class Strategy : NonReactiveResilienceStrategy
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

    private class ExecuteCallbackTwiceStrategy : NonReactiveResilienceStrategy
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

    private class InvalidResilienceStrategyOptions : ResilienceStrategyOptions
    {
        [Required]
        public string? RequiredProperty { get; set; }
    }

    private class InvalidCompositeStrategyBuilder : CompositeStrategyBuilderBase
    {
        [Required]
        public string? RequiredProperty { get; set; }
    }
}
