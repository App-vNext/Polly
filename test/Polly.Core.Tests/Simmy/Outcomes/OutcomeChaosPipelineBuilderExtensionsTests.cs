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
                Outcome = new(100)
            });

            AssertResultStrategy(builder, true, 0.6, new(100));
        }
    };

    public static readonly TheoryData<Action<ResiliencePipelineBuilder<string>>> FaultGenericStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault<string>(new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new(new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.6, new InvalidOperationException("Dummy exception."));
        }
    };

    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> FaultStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault(new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new(new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<InvalidOperationException>(builder, true, 0.6, new InvalidOperationException("Dummy exception."));
        }
    };

    private static void AssertResultStrategy<T>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate, Outcome<T> outcome)
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<T>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.OutcomeGenerator.Should().NotBeNull();
        strategy.OutcomeGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(outcome);
        strategy.Outcome.Should().Be(outcome);
    }

    private static void AssertFaultStrategy<T, TException>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate, Exception ex)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<T>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator.Should().NotBeNull();
        strategy.Fault.Should().BeOfType(typeof(Outcome<Exception>));
        strategy.Fault.Should().NotBeNull();

        // it is supposed that this line should work the same as the try/catch block, but it's not, ideas?
        //Assert.Throws<TException>(() => { var _ = strategy.Fault.Value; }).Should().Be(ex);

        try
        {
            var _ = strategy.Fault!.Value;
        }
        catch (Exception e)
        {
            e.Should().Be(ex);
        }
    }

    private static void AssertFaultStrategy<TException>(ResiliencePipelineBuilder builder, bool enabled, double injectionRate, Exception ex)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<OutcomeChaosStrategy<object>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator.Should().NotBeNull();
        strategy.Fault.Should().BeOfType(typeof(Outcome<Exception>));
        strategy.Fault.Should().NotBeNull();

        // it is supposed that this line should work the same as the try/catch block, but it's not, ideas?
        //Assert.Throws<TException>(() => { var _ = strategy.Fault.Value; }).Should().Be(ex);

        try
        {
            var _ = strategy.Fault!.Value;
        }
        catch (Exception e)
        {
            e.Should().Be(ex);
        }
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

        AssertFaultStrategy<InvalidOperationException>(builder, true, 0.5, new InvalidOperationException("Dummy exception"));
    }

    [Fact]
    public void AddFault_Shortcut_Option_Throws()
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
    public void AddFault_Generic_Shortcut_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        builder
            .AddChaosFault(true, 0.5, new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.5, new InvalidOperationException("Dummy exception"));
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Option_Throws()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosFault(true, -1, new InvalidOperationException()))
            .Should()
            .Throw<ValidationException>();
    }
}
