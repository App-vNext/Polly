using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Fault;

public class ChaosFaultPipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> FaultStrategy = new()
    {
        builder =>
        {
            builder.AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = 0.6,
                Randomizer = () => 0.5,
                FaultGenerator = _=> new ValueTask<Exception?>( new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<InvalidOperationException>(builder, true, 0.6).FaultGenerator.ShouldNotBeNull();
        },
    };
#pragma warning restore IDE0028

    private static void AssertFaultStrategy<T, TException>(ResiliencePipelineBuilder<T> builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosFaultStrategy>();

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBeOfType<TException>();
    }

    private static ChaosFaultStrategy AssertFaultStrategy<TException>(ResiliencePipelineBuilder builder, bool enabled, double injectionRate)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosFaultStrategy>();

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(injectionRate);
        strategy.FaultGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBeOfType<TException>();

        return strategy;
    }

    [MemberData(nameof(FaultStrategy))]
    [Theory]
    internal void AddFault_Options_Ok(Action<ResiliencePipelineBuilder> configure)
    {
        var builder = new ResiliencePipelineBuilder();
        Should.NotThrow(() => configure(builder));
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Generator_Option_Throws() =>
        Should.Throw<ValidationException>(
            () =>
                new ResiliencePipelineBuilder<int>()
                .AddChaosFault(1.5, () => new InvalidOperationException()));

    [Fact]
    public void AddFault_Shortcut_Generator_Option_Throws() =>
        Should.Throw<ValidationException>(
            () =>
                new ResiliencePipelineBuilder()
                .AddChaosFault(1.5, () => new InvalidOperationException()));

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
