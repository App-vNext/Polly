namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResilienceStrategyTests
{
    public static TheoryData<Func<ResilienceStrategy<string>, ValueTask>> ExecuteAsyncGenericStrategyData = new()
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
            var context = ResilienceContext.Get();
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
            var context = ResilienceContext.Get();
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
    public async Task ExecuteAsync_GenericStrategy_Ok(Func<ResilienceStrategy<string>, ValueTask> execute)
    {
        var strategy = new ResilienceStrategy<string>(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.Should().BeFalse();
                c.ResultType.Should().Be(typeof(string));
                c.CancellationToken.CanBeCanceled.Should().BeTrue();
            },
        });

        await execute(strategy);
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_GenericStrategy_Ok()
    {
        var result = await NullResilienceStrategy<int>.Instance.ExecuteOutcomeAsync((context, state) =>
        {
            state.Should().Be("state");
            context.IsSynchronous.Should().BeFalse();
            context.ResultType.Should().Be(typeof(int));
            return new Outcome<int>(12345).AsValueTask();
        },
        ResilienceContext.Get(),
        "state");

        result.Result.Should().Be(12345);
    }
}
