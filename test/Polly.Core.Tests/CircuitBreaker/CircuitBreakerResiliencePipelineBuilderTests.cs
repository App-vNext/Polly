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

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<CircuitBreakerResilienceStrategy<object>>();
    }

    [MemberData(nameof(ConfigureDataGeneric))]
    [Theory]
    public void AddCircuitBreaker_Generic_Configure(Action<ResiliencePipelineBuilder<int>> builderAction)
    {
        var builder = new ResiliencePipelineBuilder<int>();

        builderAction(builder);

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<CircuitBreakerResilienceStrategy<int>>();
    }

    [Fact]
    public void AddCircuitBreaker_Validation()
    {
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder<int>().AddCircuitBreaker(new CircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }));
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions { BreakDuration = TimeSpan.MinValue }));
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
        opened.ShouldBe(1);
        halfOpened.ShouldBe(0);
        closed.ShouldBe(0);
        BrokenCircuitException exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.ShouldBe(breakDuration);

        // Circuit still open after some time
        timeProvider.Advance(halfBreakDuration);
        opened.ShouldBe(1);
        halfOpened.ShouldBe(0);
        closed.ShouldBe(0);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.ShouldBe(halfBreakDuration);

        // Circuit Half Opened
        timeProvider.Advance(halfBreakDuration);
        strategy.Execute(_ => -1, cancellationToken);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        opened.ShouldBe(2);
        halfOpened.ShouldBe(1);
        closed.ShouldBe(0);
        exception.RetryAfter.ShouldBe(breakDuration);

        // Now close it
        timeProvider.Advance(breakDuration);
        strategy.Execute(_ => 0, cancellationToken);
        opened.ShouldBe(2);
        halfOpened.ShouldBe(2);
        closed.ShouldBe(1);
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
        opened.ShouldBe(1);
        halfOpened.ShouldBe(0);
        closed.ShouldBe(0);
        BrokenCircuitException exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.ShouldBe(breakDuration);

        // Circuit still open after some time
        timeProvider.Advance(halfBreakDuration);
        opened.ShouldBe(1);
        halfOpened.ShouldBe(0);
        closed.ShouldBe(0);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        exception.RetryAfter.ShouldBe(halfBreakDuration);

        // Circuit Half Opened
        timeProvider.Advance(halfBreakDuration);
        strategy.Execute(_ => -1, cancellationToken);
        exception = Assert.Throws<BrokenCircuitException>(() => strategy.Execute(_ => 0, cancellationToken));
        opened.ShouldBe(2);
        halfOpened.ShouldBe(1);
        closed.ShouldBe(0);
        exception.RetryAfter.ShouldBe(breakDuration);

        // Now close it
        timeProvider.Advance(breakDuration);
        strategy.Execute(_ => 0, cancellationToken);
        opened.ShouldBe(2);
        halfOpened.ShouldBe(2);
        closed.ShouldBe(1);
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

        Should.Throw<IsolatedCircuitException>(() => strategy1.Execute(() => { })).RetryAfter.ShouldBeNull();
        Should.Throw<IsolatedCircuitException>(() => strategy2.Execute(() => { })).RetryAfter.ShouldBeNull();

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
            manualControl!.IsEmpty.ShouldBeFalse();
        }

        var strategy = (ResilienceStrategy<object>)pipeline.GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        await pipeline.DisposeHelper.DisposeAsync();

        Should.Throw<ObjectDisposedException>(() => strategy.AsPipeline().Execute(() => 1));

        if (attachManualControl)
        {
            manualControl!.IsEmpty.ShouldBeTrue();
        }
    }
}
