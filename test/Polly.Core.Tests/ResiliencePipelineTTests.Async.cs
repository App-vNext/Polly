using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
#pragma warning disable IDE0028
    public static List<Func<ResiliencePipeline<string>, ValueTask>> ExecuteAsyncGenericStrategyData = new()
    {
        async strategy =>
        {
            (await strategy.ExecuteAsync(
                token =>
                {
                    token.ShouldBe(CancellationToken);
                    return new ValueTask<string>("res");
                },
                CancellationToken)).ShouldBe("res");
        },

        async strategy =>
        {
            (await strategy.ExecuteAsync(
                (state, token) =>
                {
                    state.ShouldBe("state");
                    token.ShouldBe(CancellationToken);
                    return new ValueTask<string>("res");
                },
                "state",
                CancellationToken)).ShouldBe("res");
        },

        async strategy =>
        {
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
            context.CancellationToken = CancellationToken;
            (await strategy.ExecuteAsync(
                (context, state) =>
                {
                    state.ShouldBe("state");
                    context.ShouldBe(context);
                    return new ValueTask<string>("res");
                },
                context,
                "state")).ShouldBe("res");
        },

        async strategy =>
        {
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
            context.CancellationToken = CancellationToken;
            (await strategy.ExecuteAsync(
                (context) =>
                {
                    context.ShouldBe(context);
                    return new ValueTask<string>("res");
                },
                context)).ShouldBe("res");
        },
    };
#pragma warning restore IDE0028

    [Theory]
#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    //[MemberData(nameof(ExecuteAsyncGenericStrategyData))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    public async Task ExecuteAsync_GenericStrategy_Ok(int index)
    {
        Func<ResiliencePipeline<string>, ValueTask> execute = ExecuteAsyncGenericStrategyData[index];

        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.ShouldBeFalse();
                c.ResultType.ShouldBe(typeof(string));
                c.CancellationToken.CanBeCanceled.ShouldBeTrue();
            },
        }), DisposeBehavior.Allow, null);

        await execute(pipeline);
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_GenericStrategy_Ok()
    {
        var result = await ResiliencePipeline<int>.Empty.ExecuteOutcomeAsync((context, state) =>
        {
            state.ShouldBe("state");
            context.IsSynchronous.ShouldBeFalse();
            context.ResultType.ShouldBe(typeof(int));
            return Outcome.FromResultAsValueTask(12345);
        },
        ResilienceContextPool.Shared.Get(CancellationToken),
        "state");

        result.Result.ShouldBe(12345);
    }
}
