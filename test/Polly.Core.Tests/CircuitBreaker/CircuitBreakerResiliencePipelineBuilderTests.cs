using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker;
using Polly.Testing;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResiliencePipelineBuilderTests
{
#pragma warning disable IDE0028
    public static TheoryData<Action<ResiliencePipelineBuilder>> ConfigureData = new()
    {
        builder => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            ShouldHandle = _ => PredicateResult.True()
        }),
    };

    public static TheoryData<Action<ResiliencePipelineBuilder<int>>> ConfigureDataGeneric = new()
    {
        builder => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<int>
        {
            ShouldHandle = _ => PredicateResult.True()
        }),
    };
#pragma warning restore IDE0028

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
        var cancellationToken = CancellationToken.None;
        int opened = 0;
        int closed = 0;
        int halfOpened = 0;

        var halfBreakDuration = TimeSpan.FromMilliseconds(500);
        var breakDuration = halfBreakDuration + halfBreakDuration;

        var options = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(10),
            BreakDuration = breakDuration,
            ShouldHandle = args => new ValueTask<bool>(args.Outcome.Result is -1),
            OnOpened = _ => { opened++; return default; },
            OnClosed = _ => { closed++; return default; },
            OnHalfOpened = (_) => { halfOpened++; return default; }
        };

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResiliencePipelineBuilder { TimeProvider = timeProvider }.AddCircuitBreaker(options).Build();

        for (int i = 0; i < 10; i++)
        {
            strategy.Execute(_ => -1, cancellationToken);
        }

        // Circuit opened
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        BrokenCircuitException exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.Should().Be(breakDuration);

        // Circuit still open after some time
        timeProvider.Advance(halfBreakDuration);
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.Should().Be(halfBreakDuration);

        // Circuit Half Opened
        timeProvider.Advance(halfBreakDuration);
        strategy.Execute(_ => -1, cancellationToken);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        opened.Should().Be(2);
        halfOpened.Should().Be(1);
        closed.Should().Be(0);
        exception.RetryAfter.Should().Be(breakDuration);

        // Now close it
        timeProvider.Advance(breakDuration);
        strategy.Execute(_ => 0, cancellationToken);
        opened.Should().Be(2);
        halfOpened.Should().Be(2);
        closed.Should().Be(1);
    }

    [Fact]
    public void AddCircuitBreaker_IntegrationTest_WithBreakDurationGenerator()
    {
        var cancellationToken = CancellationToken.None;
        int opened = 0;
        int closed = 0;
        int halfOpened = 0;

        var halfBreakDuration = TimeSpan.FromMilliseconds(500);
        var breakDuration = halfBreakDuration + halfBreakDuration;

        var options = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(10),
            BreakDuration = TimeSpan.FromSeconds(30), // Intentionally long to check it isn't used
            BreakDurationGenerator = (_) => new ValueTask<TimeSpan>(breakDuration),
            ShouldHandle = args => new ValueTask<bool>(args.Outcome.Result is -1),
            OnOpened = _ => { opened++; return default; },
            OnClosed = _ => { closed++; return default; },
            OnHalfOpened = (_) => { halfOpened++; return default; }
        };

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResiliencePipelineBuilder { TimeProvider = timeProvider }.AddCircuitBreaker(options).Build();

        for (int i = 0; i < 10; i++)
        {
            strategy.Execute(_ => -1, cancellationToken);
        }

        // Circuit opened
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        BrokenCircuitException exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.Should().Be(breakDuration);

        // Circuit still open after some time
        timeProvider.Advance(halfBreakDuration);
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.Should().Be(halfBreakDuration);

        // Circuit Half Opened
        timeProvider.Advance(halfBreakDuration);
        strategy.Execute(_ => -1, cancellationToken);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        opened.Should().Be(2);
        halfOpened.Should().Be(1);
        closed.Should().Be(0);
        exception.RetryAfter.Should().Be(breakDuration);

        // Now close it
        timeProvider.Advance(breakDuration);
        strategy.Execute(_ => 0, cancellationToken);
        opened.Should().Be(2);
        halfOpened.Should().Be(2);
        closed.Should().Be(1);
    }

    [Fact]
    public async Task AddCircuitBreakers_WithIsolatedManualControl_ShouldBeIsolated()
    {
        var cancellationToken = CancellationToken.None;
        var manualControl = new CircuitBreakerManualControl();
        await manualControl.IsolateAsync(cancellationToken);

        var strategy1 = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() { ManualControl = manualControl })
            .Build();

        var strategy2 = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() { ManualControl = manualControl })
            .Build();

        strategy1.Invoking(s => s.Execute(() => { })).Should().Throw<IsolatedCircuitException>().Where(e => e.RetryAfter == null);
        strategy2.Invoking(s => s.Execute(() => { })).Should().Throw<IsolatedCircuitException>().Where(e => e.RetryAfter == null);

        await manualControl.CloseAsync(cancellationToken);

        strategy1.Execute(() => { });
        strategy2.Execute(() => { });
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public async Task DisposePipeline_EnsureCircuitBreakerDisposed(bool attachManualControl)
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

        await pipeline.DisposeHelper.DisposeAsync();

        strategy.AsPipeline().Invoking(s => s.Execute(() => 1)).Should().Throw<ObjectDisposedException>();

        if (attachManualControl)
        {
            manualControl!.IsEmpty.Should().BeTrue();
        }
    }
}
