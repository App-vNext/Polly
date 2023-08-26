using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Telemetry;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

#pragma warning disable CA2000 // Dispose objects before losing scope

public class CompositePipelineComponentTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly FakeTelemetryListener _listener;

    public CompositePipelineComponentTests()
    {
        _listener = new FakeTelemetryListener();
        _telemetry = TestUtilities.CreateResilienceTelemetry(_listener);
    }

    [Fact]
    public void Create_EnsureOriginalStrategiesPreserved()
    {
        var pipelines = new[]
        {
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
        };

        var pipeline = CreateSut(pipelines);

        for (var i = 0; i < pipelines.Length; i++)
        {
            pipeline.Components[i].Should().BeSameAs(pipelines[i]);
        }

        pipeline.Components.SequenceEqual(pipelines).Should().BeTrue();
    }

    [Fact]
    public async Task Create_EnsureExceptionsNotWrapped()
    {
        var components = new[]
        {
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy { Before =  (_, _) => throw new NotSupportedException() }),
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy { Before =  (_, _) => throw new NotSupportedException() }),
        };

        var pipeline = CreateSut(components);
        await pipeline
            .Invoking(p => p.ExecuteCore((_, _) => Outcome.FromResultAsTask(10), ResilienceContextPool.Shared.Get(), "state").AsTask())
            .Should()
            .ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void Create_EnsurePipelineReusableAcrossDifferentPipelines()
    {
        var components = new[]
        {
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()),
            Substitute.For<PipelineComponent>(),
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()),

        };

        var pipeline = CreateSut(components);

        CreateSut(new PipelineComponent[] { PipelineComponent.Empty, pipeline });

        this.Invoking(_ => CreateSut(new PipelineComponent[] { PipelineComponent.Empty, pipeline }))
            .Should()
            .NotThrow();
    }

    [Fact]
    public async Task Create_Cancelled_EnsureNoExecution()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var strategies = new[]
        {
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()),
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()),
        };

        var pipeline = new ResiliencePipeline(CreateSut(strategies, new FakeTimeProvider()), DisposeBehavior.Allow);
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("result"), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [Fact]
    public async Task Create_CancelledLater_EnsureNoExecution()
    {
        var executed = false;
        using var cancellation = new CancellationTokenSource();
        var strategies = new[]
        {
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } }),
            PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()),
        };
        var pipeline = new ResiliencePipeline(CreateSut(strategies, new FakeTimeProvider()), DisposeBehavior.Allow);
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("result"), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecutePipeline_EnsureTelemetryArgumentsReported()
    {
        var timeProvider = new FakeTimeProvider();

        var pipeline = new ResiliencePipeline(CreateSut(new[] { Substitute.For<PipelineComponent>() }, timeProvider), DisposeBehavior.Allow);
        pipeline.Execute(() => { timeProvider.Advance(TimeSpan.FromHours(1)); });

        _listener.Events.Should().HaveCount(2);
        _listener.GetArgs<PipelineExecutingArguments>().Should().HaveCount(1);
        _listener.GetArgs<PipelineExecutedArguments>().Should().HaveCount(1);
    }

    [Fact]
    public void Dispose_EnsureInnerComponentsDisposed()
    {
        var a = Substitute.For<PipelineComponent>();
        var b = Substitute.For<PipelineComponent>();

        var composite = CreateSut(new[] { a, b });

        composite.FirstComponent.Dispose();
        composite.Dispose();

        a.Received(1).Dispose();
        b.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_EnsureInnerComponentsDisposed()
    {
        var a = Substitute.For<PipelineComponent>();
        var b = Substitute.For<PipelineComponent>();

        var composite = CreateSut(new[] { a, b });
        await composite.FirstComponent.DisposeAsync();
        await composite.DisposeAsync();

        await a.Received(1).DisposeAsync();
        await b.Received(1).DisposeAsync();
    }

    private CompositeComponent CreateSut(PipelineComponent[] components, TimeProvider? timeProvider = null)
    {
        return (CompositeComponent)PipelineComponentFactory.CreateComposite(components, _telemetry, timeProvider ?? Substitute.For<TimeProvider>());
    }
}
