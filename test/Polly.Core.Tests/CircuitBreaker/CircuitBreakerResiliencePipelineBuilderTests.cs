using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker;
using Polly.Testing;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResiliencePipelineBuilderTests
{
    public static TheoryData<Action<ResiliencePipelineBuilder>> ConfigureData = new()
    {
        builder => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            ShouldHandle = _ => PredicateResult.True
        }),
    };

    public static TheoryData<Action<ResiliencePipelineBuilder<int>>> ConfigureDataGeneric = new()
    {
        builder => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<int>
        {
            ShouldHandle = _ => PredicateResult.True
        }),
    };

    [MemberData(nameof(ConfigureData))]
    [Theory]
    public void AddCircuitBreaker_Configure(Action<ResiliencePipelineBuilder> builderAction)
    {
        var builder = new ResiliencePipelineBuilder();

        builderAction(builder);

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<CircuitBreakerResilienceStrategy<object>>();
    }

    [MemberData(nameof(ConfigureDataGeneric))]
    [Theory]
    public void AddCircuitBreaker_Generic_Configure(Action<ResiliencePipelineBuilder<int>> builderAction)
    {
        var builder = new ResiliencePipelineBuilder<int>();

        builderAction(builder);

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<CircuitBreakerResilienceStrategy<int>>();
    }

    [Fact]
    public void AddCircuitBreaker_Validation()
    {
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddCircuitBreaker(new CircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>();

        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddCircuitBreaker(new CircuitBreakerStrategyOptions { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddCircuitBreaker_IntegrationTest()
    {
        int opened = 0;
        int closed = 0;
        int halfOpened = 0;

        var options = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(10),
            BreakDuration = TimeSpan.FromSeconds(1),
            ShouldHandle = args => new ValueTask<bool>(args.Result is -1),
            OnOpened = _ => { opened++; return default; },
            OnClosed = _ => { closed++; return default; },
            OnHalfOpened = (_) => { halfOpened++; return default; }
        };

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResiliencePipelineBuilder { TimeProvider = timeProvider }.AddCircuitBreaker(options).Build();

        for (int i = 0; i < 10; i++)
        {
            strategy.Execute(_ => -1);
        }

        // Circuit opened
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        Assert.Throws<BrokenCircuitException<object>>(() => strategy.Execute(_ => 0));

        // Circuit Half Opened
        timeProvider.Advance(options.BreakDuration);
        strategy.Execute(_ => -1);
        Assert.Throws<BrokenCircuitException<object>>(() => strategy.Execute(_ => 0));
        opened.Should().Be(2);
        halfOpened.Should().Be(1);
        closed.Should().Be(0);

        // Now close it
        timeProvider.Advance(options.BreakDuration);
        strategy.Execute(_ => 0);
        opened.Should().Be(2);
        halfOpened.Should().Be(2);
        closed.Should().Be(1);
    }

    [Fact]
    public async Task AddCircuitBreakers_WithIsolatedManualControl_ShouldBeIsolated()
    {
        var manualControl = new CircuitBreakerManualControl();
        await manualControl.IsolateAsync();

        var strategy1 = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() { ManualControl = manualControl })
            .Build();

        var strategy2 = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() { ManualControl = manualControl })
            .Build();

        strategy1.Invoking(s => s.Execute(() => { })).Should().Throw<IsolatedCircuitException>();
        strategy2.Invoking(s => s.Execute(() => { })).Should().Throw<IsolatedCircuitException>();

        await manualControl.CloseAsync();

        strategy1.Execute(() => { });
        strategy2.Execute(() => { });
    }

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [Theory]
    public async Task DisposePipeline_EnsureCircuitBreakerDisposed(bool isAsync, bool attachManualControl)
    {
        var manualControl = attachManualControl ? new CircuitBreakerManualControl() : null;
        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ManualControl = manualControl
            })
            .Build();

        if (attachManualControl)
        {
            manualControl!.IsEmpty.Should().BeFalse();
        }

        var strategy = (ResilienceStrategy<object>)pipeline.GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        if (isAsync)
        {
            await pipeline.DisposeHelper.DisposeAsync();
        }
        else
        {
            pipeline.DisposeHelper.Dispose();
        }

        strategy.AsPipeline().Invoking(s => s.Execute(() => 1)).Should().Throw<ObjectDisposedException>();

        if (attachManualControl)
        {
            manualControl!.IsEmpty.Should().BeTrue();
        }
    }
}
