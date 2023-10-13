using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Outcomes;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeChaosPipelineBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResiliencePipelineBuilder<int>>> ResultStrategy = new()
    {
        builder =>
        {
            builder.AddChaosResult(new OutcomeStrategyOptions<int>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new Outcome<int>(100)
            });

            AssertResultStrategy(builder, true, 0.6, new(100))
            .Outcome.Should().Be(new Outcome<int>(100));
        }
    };

    public static readonly TheoryData<Action<ResiliencePipelineBuilder<string>>> FaultGenericStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault(new FaultStrategyOptions
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Fault = new InvalidOperationException("Dummy exception.")
            });

            AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.6)
            .Fault.Should().BeOfType(typeof(InvalidOperationException));
        }
    };

    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> FaultStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault(new FaultStrategyOptions
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Fault = new InvalidOperationException("Dummy exception.")
            });

            AssertFaultStrategy<InvalidOperationException>(builder, true, 0.6)
            .Fault.Should().BeOfType(typeof(InvalidOperationException));
        }
    };

    private static OutcomeChaosStrategy<T> AssertResultStrategy<T>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate, Outcome<T> outcome)
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<T>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.OutcomeGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(outcome);

        return strategy;
    }

    private static OutcomeChaosStrategy<T> AssertFaultStrategy<T, TException>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<T>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeOfType(typeof(TException));

        return strategy;
    }

    private static OutcomeChaosStrategy<object> AssertFaultStrategy<TException>(ResiliencePipelineBuilder builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<object>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeOfType(typeof(TException));

        return strategy;
    }

    [MemberData(nameof(ResultStrategy))]
    [Theory]
    internal void AddResult_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(FaultGenericStrategy))]
    [Theory]
    internal void AddFault_Generic_Options_Ok(Action<ResiliencePipelineBuilder<string>> configure)
    {
        var builder = new ResiliencePipelineBuilder<string>();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(FaultStrategy))]
    [Theory]
    internal void AddFault_Options_Ok(Action<ResiliencePipelineBuilder> configure)
    {
        var builder = new ResiliencePipelineBuilder();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddResult_Shortcut_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<int>();
        builder
            .AddChaosResult(true, 0.5, 120)
            .Build();

        AssertResultStrategy(builder, true, 0.5, new(120));
    }

    [Fact]
    public void AddResult_Shortcut_Generator_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<int>();
        builder
            .AddChaosResult(true, 0.5, () => 120)
            .Build();

        AssertResultStrategy(builder, true, 0.5, new(120));
    }

    [Fact]
    public void AddResult_Shortcut_Option_Throws()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosResult(true, -1, () => 120))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Shortcut_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        builder
            .AddChaosFault(true, 0.5, new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<InvalidOperationException>(builder, true, 0.5);
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Generator_Option_Throws()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosFault(
                true,
                1.5,
                () => new InvalidOperationException()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Shortcut_Generator_Option_Throws()
    {
        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddChaosFault(
                true,
                1.5,
                () => new InvalidOperationException()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Shortcut_Generator_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        builder
            .AddChaosFault(true, 0.5, () => new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<InvalidOperationException>(builder, true, 0.5);
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        builder
            .AddChaosFault(true, 0.5, new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.5);
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Option_Throws()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosFault(true, -1, new InvalidOperationException()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Generator_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        builder
            .AddChaosFault(true, 0.5, () => new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.5);
    }
}
