using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
#pragma warning disable IDE0028
    public static List<Action<ResiliencePipeline<string>>> ExecuteGenericStrategyData = new()
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

    //[MemberData(nameof(ExecuteGenericStrategyData))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [Theory]
    public void Execute_GenericStrategy_Ok(int index)
    {
        Action<ResiliencePipeline<string>> execute = ExecuteGenericStrategyData[index];
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
}
