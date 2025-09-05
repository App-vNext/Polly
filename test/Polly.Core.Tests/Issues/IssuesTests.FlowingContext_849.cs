using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void FlowingContext_849()
    {
        var contextChecked = false;
        var strategy = new ResiliencePipelineBuilder<int>()
            .AddRetry(new RetryStrategyOptions<int>
            {
                // configure the predicate and use the context
                ShouldHandle = args =>
                {
                    // access the context to evaluate the retry
                    ResilienceContext context = args.Context;
                    context.ShouldNotBeNull();
                    contextChecked = true;
                    return PredicateResult.False();
                }
            })
            .Build();

        // execute the retry
        strategy.Execute(_ => 0, TestCancellation.Token);

        contextChecked.ShouldBeTrue();
    }
}
