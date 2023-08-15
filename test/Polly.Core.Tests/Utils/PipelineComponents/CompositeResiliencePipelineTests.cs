using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Utils.PipelineComponents;

public class CompositeResiliencePipelineTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private Action<TelemetryEventArguments<object, object>>? _onTelemetry;

    public CompositeResiliencePipelineTests()
        => _telemetry = TestUtilities.CreateResilienceTelemetry(args => _onTelemetry?.Invoke(args));

    [Fact]
    public void Create_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => PipelineComponent.CreateComposite(null!, null!, null!));
        Assert.Throws<InvalidOperationException>(() => PipelineComponent.CreateComposite(Array.Empty<PipelineComponent>(), null!, null!));
        Assert.Throws<InvalidOperationException>(() => PipelineComponent.CreateComposite(new[] { PipelineComponent.Null, PipelineComponent.Null, }, null!, null!));
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
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
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
            PipelineComponent.FromStrategy(new TestResilienceStrategy()),
            Substitute.For<PipelineComponent>(),
            PipelineComponent.FromStrategy(new TestResilienceStrategy()),

        };

        var pipeline = CreateSut(components);

        CreateSut(new PipelineComponent[] { PipelineComponent.Null, pipeline });

        this.Invoking(_ => CreateSut(new PipelineComponent[] { PipelineComponent.Null, pipeline }))
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
            PipelineComponent.FromStrategy(new TestResilienceStrategy()),
            PipelineComponent.FromStrategy(new TestResilienceStrategy()),
        };

        var pipeline = new ResiliencePipeline(CreateSut(strategies, new FakeTimeProvider()));
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
            PipelineComponent.FromStrategy( new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } }),
            PipelineComponent.FromStrategy(new TestResilienceStrategy()),
        };
        var pipeline = new ResiliencePipeline(CreateSut(strategies, new FakeTimeProvider()));
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("result"), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecutePipeline_EnsureTelemetryArgumentsReported()
    {
        var items = new List<object>();
        var timeProvider = new FakeTimeProvider();

        _onTelemetry = args =>
        {
            if (args.Arguments is PipelineExecutedArguments executed)
            {
                executed.Duration.Should().Be(TimeSpan.FromHours(1));
            }

            items.Add(args.Arguments);
        };

        var pipeline = new ResiliencePipeline(CreateSut(new[] { Substitute.For<PipelineComponent>() }, timeProvider));
        pipeline.Execute(() => { timeProvider.Advance(TimeSpan.FromHours(1)); });

        items.Should().HaveCount(2);
        items[0].Should().BeOfType<PipelineExecutingArguments>();
        items[1].Should().BeOfType<PipelineExecutedArguments>();
    }

    private PipelineComponent.CompositeComponent CreateSut(PipelineComponent[] components, TimeProvider? timeProvider = null)
    {
        return (PipelineComponent.CompositeComponent)PipelineComponent.CreateComposite(components, _telemetry, timeProvider ?? Substitute.For<TimeProvider>());
    }
}
