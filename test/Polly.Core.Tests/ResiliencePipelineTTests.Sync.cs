using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
    public static TheoryData<Action<ResiliencePipeline<string>>> ExecuteGenericStrategyData = new()
    {
        strategy =>
        {
            strategy.Execute(() => "res").Should().Be("res");
        },

        strategy =>
        {
            strategy.Execute(state =>
            {
                state.Should().Be("state");
                return "res";
            },
            "state").Should().Be("res");
        },

        strategy =>
        {
            strategy.Execute(
                token =>
                {
                    token.Should().Be(CancellationToken);
                    return "res";
                },
                CancellationToken).Should().Be("res");
        },

        strategy =>
        {
            strategy.Execute(
                (state, token) =>
                {
                    state.Should().Be("state");
                    token.Should().Be(CancellationToken);
                    return "res";
                },
                "state",
                CancellationToken).Should().Be("res");
        },

        strategy =>
        {
            var context = ResilienceContextPool.Shared.Get();
            context.CancellationToken = CancellationToken;
            strategy.Execute(
                (context, state) =>
                {
                    state.Should().Be("state");
                    context.Should().Be(context);
                    context.CancellationToken.Should().Be(CancellationToken);
                    return "res";
                },
                context,
                "state").Should().Be("res");
        },

        strategy =>
        {
            var context = ResilienceContextPool.Shared.Get();
            context.CancellationToken = CancellationToken;
            strategy.Execute(
                (context) =>
                {
                    context.Should().Be(context);
                    return "res";
                },
                context).Should().Be("res");
        }
    };

    [MemberData(nameof(ExecuteGenericStrategyData))]
    [Theory]
    public void Execute_GenericStrategy_Ok(Action<ResiliencePipeline<string>> execute)
    {
        var pipeline = new ResiliencePipeline<string>(PipelineComponentFactory.FromStrategy(new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                c.IsSynchronous.Should().BeTrue();
                c.ResultType.Should().Be(typeof(string));
            },
        }), DisposeBehavior.Allow);

        execute(pipeline);
    }
}
