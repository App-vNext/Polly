using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class CompositeResiliencePipelineTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private Action<TelemetryEventArguments<object, object>>? _onTelemetry;

    public CompositeResiliencePipelineTests()
        => _telemetry = TestUtilities.CreateResilienceTelemetry(args => _onTelemetry?.Invoke(args));

    [Fact]
    public void Create_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => CompositeResiliencePipeline.Create(null!, null!, null!));
        Assert.Throws<InvalidOperationException>(() => CompositeResiliencePipeline.Create(Array.Empty<ResiliencePipeline>(), null!, null!));
        Assert.Throws<InvalidOperationException>(() => CompositeResiliencePipeline.Create(new ResiliencePipeline[]
        {
            NullResiliencePipeline.Instance,
            NullResiliencePipeline.Instance
        }, null!, null!));
    }

    [Fact]
    public void Create_EnsureOriginalStrategiesPreserved()
    {
        var strategies = new[]
        {
            new TestResilienceStrategy().AsPipeline(),
            new Strategy().AsPipeline(),
            new TestResilienceStrategy().AsPipeline(),
        };

        var pipeline = CreateSut(strategies);

        for (var i = 0; i < strategies.Length; i++)
        {
            pipeline.Strategies[i].Should().BeSameAs(strategies[i]);
        }

        pipeline.Strategies.SequenceEqual(strategies).Should().BeTrue();
    }

    [Fact]
    public async Task Create_EnsureExceptionsNotWrapped()
    {
        var strategies = new[]
        {
            new Strategy().AsPipeline(),
            new Strategy().AsPipeline(),
        };

        var pipeline = CreateSut(strategies);
        await pipeline
            .Invoking(p => p.ExecuteCore((_, _) => Outcome.FromResultAsTask(10), ResilienceContextPool.Shared.Get(), "state").AsTask())
            .Should()
            .ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void Create_EnsurePipelineReusableAcrossDifferentPipelines()
    {
        var strategies = new[]
        {
            new TestResilienceStrategy().AsPipeline(),
            new Strategy().AsPipeline(),
            new TestResilienceStrategy().AsPipeline(),
        };

        var pipeline = CreateSut(strategies);

        CreateSut(new ResiliencePipeline[] { NullResiliencePipeline.Instance, pipeline });

        this.Invoking(_ => CreateSut(new ResiliencePipeline[] { NullResiliencePipeline.Instance, pipeline }))
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
            new TestResilienceStrategy().AsPipeline(),
            new TestResilienceStrategy().AsPipeline(),
        };

        var pipeline = CreateSut(strategies, new FakeTimeProvider());
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
            new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } }.AsPipeline(),
            new TestResilienceStrategy().AsPipeline(),
        };
        var pipeline = CreateSut(strategies, new FakeTimeProvider());
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("result"), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecuptePipeline_EnsureTelemetryArgumentsReported()
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

        var pipeline = CreateSut(new[] { new TestResilienceStrategy().AsPipeline() }, timeProvider);
        pipeline.Execute(() => { timeProvider.Advance(TimeSpan.FromHours(1)); });

        items.Should().HaveCount(2);
        items[0].Should().BeOfType<PipelineExecutingArguments>();
        items[1].Should().BeOfType<PipelineExecutedArguments>();
    }

    private CompositeResiliencePipeline CreateSut(ResiliencePipeline[] strategies, TimeProvider? timeProvider = null)
    {
        return CompositeResiliencePipeline.Create(strategies, _telemetry, timeProvider ?? Substitute.For<TimeProvider>());
    }

    private class Strategy : ResilienceStrategy
    {
        protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            await callback(context, state);

            throw new NotSupportedException();
        }
    }
}
