using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
#pragma warning disable IDE0028
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
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
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
            var context = ResilienceContextPool.Shared.Get(CancellationToken);
            context.CancellationToken = CancellationToken;
            (await strategy.ExecuteAsync(
                (context) =>
                {
                    context.Should().Be(context);
                    return new ValueTask<string>("res");
                },
                context)).Should().Be("res");
        },
    };
#pragma warning restore IDE0028

    [Theory]
#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    [MemberData(nameof(ExecuteAsyncGenericStrategyData))]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    public async Task ExecuteAsync_GenericStrategy_Ok(Func<ResiliencePipeline<string>, ValueTask> execute)
    {
        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.Should().BeFalse();
                c.ResultType.Should().Be<string>();
                c.CancellationToken.CanBeCanceled.Should().BeTrue();
            },
        }), DisposeBehavior.Allow, null);

        await execute(pipeline);
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_GenericStrategy_Ok()
    {
        var result = await ResiliencePipeline<int>.Empty.ExecuteOutcomeAsync((context, state) =>
        {
            state.Should().Be("state");
            context.IsSynchronous.Should().BeFalse();
            context.ResultType.Should().Be<int>();
            return Outcome.FromResultAsValueTask(12345);
        },
        ResilienceContextPool.Shared.Get(CancellationToken),
        "state");

        result.Result.Should().Be(12345);
    }
}
