using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class CompositeResilienceStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private Action<TelemetryEventArguments>? _onTelemetry;

    public CompositeResilienceStrategyTests()
        => _telemetry = TestUtilities.CreateResilienceTelemetry(args => _onTelemetry?.Invoke(args));

    [Fact]
    public void Create_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => CompositeResilienceStrategy.Create(null!, null!, null!));
        Assert.Throws<InvalidOperationException>(() => CompositeResilienceStrategy.Create(Array.Empty<ResilienceStrategy>(), null!, null!));
        Assert.Throws<InvalidOperationException>(() => CompositeResilienceStrategy.Create(new ResilienceStrategy[]
        {
            NullResilienceStrategy.Instance,
            NullResilienceStrategy.Instance
        }, null!, null!));
    }

    [Fact]
    public void Create_EnsureOriginalStrategiesPreserved()
    {
        var strategies = new[]
        {
            new TestResilienceStrategy().AsStrategy(),
            new Strategy().AsStrategy(),
            new TestResilienceStrategy().AsStrategy(),
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
            new Strategy().AsStrategy(),
            new Strategy().AsStrategy(),
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
            new TestResilienceStrategy().AsStrategy(),
            new Strategy().AsStrategy(),
            new TestResilienceStrategy().AsStrategy(),
        };

        var pipeline = CreateSut(strategies);

        CreateSut(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline });

        this.Invoking(_ => CreateSut(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline }))
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
            new TestResilienceStrategy().AsStrategy(),
            new TestResilienceStrategy().AsStrategy(),
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
            new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } }.AsStrategy(),
            new TestResilienceStrategy().AsStrategy(),
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

        var pipeline = CreateSut(new[] { new TestResilienceStrategy().AsStrategy() }, timeProvider);
        pipeline.Execute(() => { timeProvider.Advance(TimeSpan.FromHours(1)); });

        items.Should().HaveCount(2);
        items[0].Should().Be(PipelineExecutingArguments.Instance);
        items[1].Should().BeOfType<PipelineExecutedArguments>();
    }

    private CompositeResilienceStrategy CreateSut(ResilienceStrategy[] strategies, TimeProvider? timeProvider = null)
    {
        return CompositeResilienceStrategy.Create(strategies, _telemetry, timeProvider ?? Substitute.For<TimeProvider>());
    }

    private class Strategy : NonReactiveResilienceStrategy
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
