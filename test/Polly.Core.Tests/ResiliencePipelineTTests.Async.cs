namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResiliencePipelineTests
{
    public static TheoryData<Func<ResiliencePipeline<string>, ValueTask>> ExecuteAsyncGenericStrategyData = new()
    {
        async strategy =>
        {
            (await strategy.ExecuteAsync(
                token =>
                {
                    token.Should().Be(CancellationToken);
                    return new ValueTask<string>("res");
                },
                CancellationToken)).Should().Be("res");
        },

        async strategy =>
        {
            (await strategy.ExecuteAsync(
                (state, token) =>
                {
                    state.Should().Be("state");
                    token.Should().Be(CancellationToken);
                    return new ValueTask<string>("res");
                },
                "state",
                CancellationToken)).Should().Be("res");
        },

        async strategy =>
        {
            var context = ResilienceContextPool.Shared.Get();
            context.CancellationToken = CancellationToken;
            (await strategy.ExecuteAsync(
                (context, state) =>
                {
                    state.Should().Be("state");
                    context.Should().Be(context);
                    return new ValueTask<string>("res");
                },
                context,
                "state")).Should().Be("res");
        },

        async strategy =>
        {
            var context = ResilienceContextPool.Shared.Get();
            context.CancellationToken = CancellationToken;
            (await strategy.ExecuteAsync(
                (context) =>
                {
                    context.Should().Be(context);
                    return new ValueTask<string>("res");
                },
                context)).Should().Be("res");
        }
    };

    [MemberData(nameof(ExecuteAsyncGenericStrategyData))]
    [Theory]
    public async Task ExecuteAsync_GenericStrategy_Ok(Func<ResiliencePipeline<string>, ValueTask> execute)
    {
        var strategy = new ResiliencePipeline<string>(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.Should().BeFalse();
                c.ResultType.Should().Be(typeof(string));
                c.CancellationToken.CanBeCanceled.Should().BeTrue();
            },
        }.AsPipeline());

        await execute(strategy);
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_GenericStrategy_Ok()
    {
        var result = await NullResiliencePipeline<int>.Instance.ExecuteOutcomeAsync((context, state) =>
        {
            state.Should().Be("state");
            context.IsSynchronous.Should().BeFalse();
            context.ResultType.Should().Be(typeof(int));
            return Outcome.FromResultAsTask(12345);
        },
        ResilienceContextPool.Shared.Get(),
        "state");

        result.Result.Should().Be(12345);
    }
}
