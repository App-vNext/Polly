using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultChaosPipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> FaultStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault(new FaultStrategyOptions
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                FaultGenerator = _=> new ValueTask<Exception?>( new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<InvalidOperationException>(builder, true, 0.6).FaultGenerator.Should().NotBeNull();
        },
    };
#pragma warning restore IDE0028

    private static void AssertFaultStrategy<T, TException>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<FaultChaosStrategy>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeOfType(typeof(TException));
    }

    private static FaultChaosStrategy AssertFaultStrategy<TException>(ResiliencePipelineBuilder builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<FaultChaosStrategy>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeOfType(typeof(TException));

        return strategy;
    }

    [MemberData(nameof(FaultStrategy))]
    [Theory]
    internal void AddFault_Options_Ok(Action<ResiliencePipelineBuilder> configure)
    {
        var builder = new ResiliencePipelineBuilder();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Generator_Option_Throws()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosFault(
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
            .AddChaosFault(0.5, () => new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<InvalidOperationException>(builder, true, 0.5);
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Generator_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        builder
            .AddChaosFault(0.5, () => new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.5);
    }
}
