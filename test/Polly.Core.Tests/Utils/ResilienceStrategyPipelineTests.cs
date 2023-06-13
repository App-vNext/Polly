using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class ResilienceStrategyPipelineTests
{
    [Fact]
    public void CreatePipeline_ArgValidation()
    {
        Assert.Throws<ArgumentNullException>(() => ResilienceStrategyPipeline.CreatePipeline(null!));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipeline(Array.Empty<ResilienceStrategy>()));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipeline(new ResilienceStrategy[] { new TestResilienceStrategy() }));
        Assert.Throws<InvalidOperationException>(() => ResilienceStrategyPipeline.CreatePipeline(new ResilienceStrategy[]
        {
            NullResilienceStrategy.Instance,
            NullResilienceStrategy.Instance
        }));
    }

    [Fact]
    public void CreatePipeline_EnsureOriginalStrategiesPreserved()
    {
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new Strategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipeline(strategies);

        for (var i = 0; i < strategies.Length; i++)
        {
            pipeline.Strategies[i].Should().BeSameAs(strategies[i]);
        }

        pipeline.Strategies.SequenceEqual(strategies).Should().BeTrue();
    }

    [Fact]
    public async Task CreatePipeline_EnsureExceptionsNotWrapped()
    {
        var strategies = new ResilienceStrategy[]
        {
            new Strategy(),
            new Strategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipeline(strategies);
        await pipeline
            .Invoking(p => p.ExecuteCoreAsync((_, _) => new Outcome<int>(10).AsValueTask(), ResilienceContext.Get(), "state").AsTask())
            .Should()
            .ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void CreatePipeline_EnsurePipelineReusableAcrossDifferentPipelines()
    {
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new Strategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipeline(strategies);

        ResilienceStrategyPipeline.CreatePipeline(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline });

        this.Invoking(_ => ResilienceStrategyPipeline.CreatePipeline(new ResilienceStrategy[] { NullResilienceStrategy.Instance, pipeline }))
            .Should()
            .NotThrow();
    }

    [Fact]
    public async Task CreatePipeline_Cancelled_EnsureNoExecution()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy(),
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipeline(strategies);
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => "result".AsOutcomeAsync(), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [Fact]
    public async Task CreatePipeline_CancelledLater_EnsureNoExecution()
    {
        var executed = false;
        using var cancellation = new CancellationTokenSource();
        var strategies = new ResilienceStrategy[]
        {
            new TestResilienceStrategy { Before = (_, _) => { executed = true; cancellation.Cancel(); } },
            new TestResilienceStrategy(),
        };

        var pipeline = ResilienceStrategyPipeline.CreatePipeline(strategies);
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellation.Token;

        var result = await pipeline.ExecuteOutcomeAsync((_, _) => "result".AsOutcomeAsync(), context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    private class Strategy : ResilienceStrategy
    {
        protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            await callback(context, state);

            throw new NotSupportedException();
        }
    }
}
