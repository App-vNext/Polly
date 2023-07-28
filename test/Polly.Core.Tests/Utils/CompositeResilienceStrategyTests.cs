using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class CompositeResilienceStrategyTests
{
    [Fact]
    public void Create_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => CompositeResilienceStrategy.Create(null!));
        Assert.Throws<InvalidOperationException>(() => CompositeResilienceStrategy.Create(Array.Empty<ResilienceStrategy>()));
        Assert.Throws<InvalidOperationException>(() => CompositeResilienceStrategy.Create(new ResilienceStrategy[] { new TestResilienceStrategy() }));
        Assert.Throws<InvalidOperationException>(() => CompositeResilienceStrategy.Create(new ResilienceStrategy[]
        {
            NullResilienceStrategy.Instance,
            NullResilienceStrategy.Instance
        }));
    }

    [Fact]
    public void Create_EnsureOriginalStrategiesPreserved()
    {
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new Strategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = CompositeResilienceStrategy.Create(strategies);

        for (var i = 0; i < strategies.Length; i++)
        {
            pipeline.Strategies[i].Should().BeSameAs(strategies[i]);
        }

        pipeline.Strategies.SequenceEqual(strategies).Should().BeTrue();
    }

    [Fact]
    public async Task Create_EnsureExceptionsNotWrapped()
    {
        var strategies = new ResilienceStrategy[]
        {
            new Strategy(),
            new Strategy(),
        };

        var pipeline = CompositeResilienceStrategy.Create(strategies);
        await pipeline
            .Invoking(p => p.ExecuteCore((_, _) => Outcome.FromResultAsTask(10), ResilienceContextPool.Shared.Get(), "state").AsTask())
            .Should()
            .ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void Create_EnsurePipelineReusableAcrossDifferentPipelines()
    {
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new Strategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = CompositeResilienceStrategy.Create(strategies);

        CompositeResilienceStrategy.Create(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline });

        this.Invoking(_ => CompositeResilienceStrategy.Create(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline }))
            .Should()
            .NotThrow();
    }

    [Fact]
    public async Task Create_Cancelled_EnsureNoExecution()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = CompositeResilienceStrategy.Create(strategies);
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
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } },
            new TestResilienceStrategy(),
        };

        var pipeline = CompositeResilienceStrategy.Create(strategies);
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("result"), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
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
