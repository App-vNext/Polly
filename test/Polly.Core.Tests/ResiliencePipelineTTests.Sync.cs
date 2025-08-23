using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
#pragma warning disable IDE0028
    public static TheoryData<Action<ResiliencePipeline<string>>> ExecuteGenericStrategyData = new()
    {
        strategy =>
        {
            strategy.Execute(() => "res").ShouldBe("res");
        },

        strategy =>
        {
            strategy.Execute(state =>
            {
                state.ShouldBe("state");
                return "res";
            },
            "state").ShouldBe("res");
        },

        strategy =>
        {
            strategy.Execute(
                token =>
                {
                    token.ShouldBe(CancellationToken);
                    return "res";
                },
                CancellationToken).ShouldBe("res");
        },

        strategy =>
        {
            strategy.Execute(
                (state, token) =>
                {
                    state.ShouldBe("state");
                    token.ShouldBe(CancellationToken);
                    return "res";
                },
                "state",
                CancellationToken).ShouldBe("res");
        },

        strategy =>
        {
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
            context.CancellationToken = CancellationToken;
            strategy.Execute(
                (context, state) =>
                {
                    state.ShouldBe("state");
                    context.ShouldBe(context);
                    context.CancellationToken.ShouldBe(CancellationToken);
                    return "res";
                },
                context,
                "state").ShouldBe("res");
        },

        strategy =>
        {
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
            context.CancellationToken = CancellationToken;
            strategy.Execute(
                (context) =>
                {
                    context.ShouldBe(context);
                    return "res";
                },
                context).ShouldBe("res");
        },
    };
#pragma warning restore IDE0028

#pragma warning disable xUnit1044
    [Theory]
    [MemberData(nameof(ExecuteGenericStrategyData))]
#pragma warning restore xUnit1044
    public void Execute_GenericStrategy_Ok(Action<ResiliencePipeline<string>> execute)
    {
        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.ShouldBeTrue();
                c.ResultType.ShouldBe(typeof(string));
            },
        }), DisposeBehavior.Allow, null);

        execute(pipeline);
    }

    [Fact]
    public void Execute_GenericStrategy_NullArgument_Throws()
    {
        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()), DisposeBehavior.Allow, null);
        var context = new ResilienceContext();

        Assert.Throws<ArgumentNullException>("callback", () => pipeline.Execute<string>(null!));
        Assert.Throws<ArgumentNullException>("callback", () => pipeline.Execute<string>(null!, context));
        Assert.Throws<ArgumentNullException>("callback", () => pipeline.Execute<string, string>(null!, context, string.Empty));
        Assert.Throws<ArgumentNullException>("context", () => pipeline.Execute((_) => string.Empty, null!));
        Assert.Throws<ArgumentNullException>("context", () => pipeline.Execute((_, _) => string.Empty, null!, string.Empty));
    }

    [Fact]
    public async Task ExecuteAsync_GenericStrategy_NullArgument_Throws()
    {
        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()), DisposeBehavior.Allow, null);
        var context = new ResilienceContext();

        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync<string>(null!, TestCancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync<string>(null!, context));
        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync<string, string>(null!, context, string.Empty));
    }

    [Fact]
    public void Execute_Strategy_NullArgument_Throws()
    {
        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()), DisposeBehavior.Allow, null);
        var context = new ResilienceContext();

        Assert.Throws<ArgumentNullException>("callback", () => pipeline.Execute(null!));
        Assert.Throws<ArgumentNullException>("callback", () => pipeline.Execute(null!, context));
        Assert.Throws<ArgumentNullException>("context", () => pipeline.Execute((_) => string.Empty, null!));
        Assert.Throws<ArgumentNullException>("context", () => pipeline.Execute((_, _) => string.Empty, null!, string.Empty));
    }

    [Fact]
    public async Task ExecuteAsync_Strategy_NullArgument_Throws()
    {
        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy()), DisposeBehavior.Allow, null);
        var context = new ResilienceContext();

        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync(null!, TestCancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync(null!, context));
        await Assert.ThrowsAsync<ArgumentNullException>("callback", async () => await pipeline.ExecuteAsync(null!, context, string.Empty));
    }
}
