using System.ComponentModel.DataAnnotations;
using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeCompositeBuilderExtensionsTests
{
    public static readonly TheoryData<Action<CompositeStrategyBuilder<int>>> ResultStrategy = new()
    {
        builder =>
        {
            builder.AddResult(new OutcomeStrategyOptions<int>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new(100)
            });

            AssertResultStrategy(builder, true, 0.6, new(100));
        }
    };

    public static readonly TheoryData<Action<CompositeStrategyBuilder<string>>> FaultGenericStrategy = new()
    {
        builder =>
        {
            builder.AddFault<string>(new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new(new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.6, new InvalidOperationException("Dummy exception."));
        }
    };

    public static readonly TheoryData<Action<CompositeStrategyBuilder>> FaultStrategy = new()
    {
        builder =>
        {
            builder.AddFault(new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 0.6,
                Enabled = true,
                Randomizer = () => 0.5,
                Outcome = new(new InvalidOperationException("Dummy exception."))
            });

            AssertFaultStrategy<InvalidOperationException>(builder, true, 0.6, new InvalidOperationException("Dummy exception."));
        }
    };

    private static void AssertResultStrategy<T>(CompositeStrategyBuilder<T> builder, bool enabled, double injectionRate, Outcome<T> outcome)
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = (OutcomeChaosStrategy<T>)builder.Build().Strategy;
        strategy.EnabledGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        strategy.OutcomeGenerator.Should().NotBeNull();
        strategy.OutcomeGenerator!.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(outcome);
        strategy.Outcome.Should().Be(outcome);
    }

    private static void AssertFaultStrategy<T, TException>(CompositeStrategyBuilder<T> builder, bool enabled, double injectionRate, Exception ex)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = (OutcomeChaosStrategy<T>)builder.Build().Strategy;
        strategy.EnabledGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
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

    private static void AssertFaultStrategy<TException>(CompositeStrategyBuilder builder, bool enabled, double injectionRate, Exception ex)
        where TException : Exception
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = (OutcomeChaosStrategy)builder.Build();
        strategy.EnabledGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(context).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
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
    internal void AddResult_Options_Ok(Action<CompositeStrategyBuilder<int>> configure)
    {
        var builder = new CompositeStrategyBuilder<int>();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(FaultGenericStrategy))]
    [Theory]
    internal void AddFault_Generic_Options_Ok(Action<CompositeStrategyBuilder<string>> configure)
    {
        var builder = new CompositeStrategyBuilder<string>();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(FaultStrategy))]
    [Theory]
    internal void AddFault_Options_Ok(Action<CompositeStrategyBuilder> configure)
    {
        var builder = new CompositeStrategyBuilder();
        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddResult_Shortcut_Option_Ok()
    {
        var builder = new CompositeStrategyBuilder<int>();
        builder
            .AddResult(true, 0.5, 120)
            .Build();

        AssertResultStrategy(builder, true, 0.5, new(120));
    }

    [Fact]
    public void AddResult_Shortcut_Option_Throws()
    {
        new CompositeStrategyBuilder<int>()
            .Invoking(b => b.AddResult(true, -1, () => new ValueTask<Outcome<int>>(Task.FromResult(new Outcome<int>(120)))))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Shortcut_Option_Ok()
    {
        var builder = new CompositeStrategyBuilder();
        builder
            .AddFault(true, 0.5, new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<InvalidOperationException>(builder, true, 0.5, new InvalidOperationException("Dummy exception"));
    }

    [Fact]
    public void AddFault_Shortcut_Option_Throws()
    {
        new CompositeStrategyBuilder<int>()
            .Invoking(b => b.AddFault(
                true,
                1.5,
                () => new ValueTask<Outcome<Exception>>(Task.FromResult(new Outcome<Exception>(new InvalidOperationException())))))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Option_Ok()
    {
        var builder = new CompositeStrategyBuilder<string>();
        builder
            .AddFault(true, 0.5, new InvalidOperationException("Dummy exception"))
            .Build();

        AssertFaultStrategy<string, InvalidOperationException>(builder, true, 0.5, new InvalidOperationException("Dummy exception"));
    }

    [Fact]
    public void AddFault_Generic_Shortcut_Option_Throws()
    {
        new CompositeStrategyBuilder<int>()
            .Invoking(b => b.AddFault(true, -1, new InvalidOperationException()))
            .Should()
            .Throw<ValidationException>();
    }
}
