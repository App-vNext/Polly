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
    [MemberData(nameof(ExecuteAsyncGenericStrategyData))]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    public async Task ExecuteAsync_GenericStrategy_Ok(Func<ResiliencePipeline<string>, ValueTask> execute)
    {
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

    [Fact]
    public async Task TryExecuteAsync_GenericStrategy_Success()
    {
        var result = await ResiliencePipeline<int>.Empty.TryExecuteAsync(
            (context, state) =>
            {
                state.ShouldBe("state");
                context.IsSynchronous.ShouldBeFalse();
                context.ResultType.ShouldBe(typeof(int));
                return new ValueTask<int>(12345);
            },
            ResilienceContextPool.Shared.Get(CancellationToken),
            "state");

        result.Result.ShouldBe(12345);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task TryExecuteAsync_GenericStrategy_Exception()
    {
        var testException = new InvalidOperationException("Test exception");

        var result = await ResiliencePipeline<int>.Empty.TryExecuteAsync<int, string>(
            (_, state) =>
            {
                state.ShouldBe("state");
                throw testException;
            },
            ResilienceContextPool.Shared.Get(CancellationToken),
            "state");

        result.Exception.ShouldBe(testException);
        result.HasResult.ShouldBeFalse();
    }

    [Fact]
    public async Task TryExecuteAsync_GenericStrategy_AsyncException()
    {
        var testException = new InvalidOperationException("Async test exception");

        var result = await ResiliencePipeline<string>.Empty.TryExecuteAsync<string, string>(
            async (_, state) =>
            {
                state.ShouldBe("state");
                await Task.Delay(1); // Make it actually async
                throw testException;
            },
            ResilienceContextPool.Shared.Get(CancellationToken),
            "state");

        result.Exception.ShouldBe(testException);
        result.HasResult.ShouldBeFalse();
    }
}
