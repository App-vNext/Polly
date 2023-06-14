using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void FlowingContext_849()
    {
        var contextChecked = false;
        var strategy = new ResilienceStrategyBuilder<int>()
            .AddRetry(new RetryStrategyOptions<int>
            {
                // configure the predicate and use the context
                ShouldRetry = args =>
                {
                    // access the context to evaluate the retry
                    ResilienceContext context = args.Context;
                    context.Should().NotBeNull();
                    contextChecked = true;
                    return PredicateResult.False;
                }
            })
            .Build();

        // execute the retry
        strategy.Execute(_ => 0);

        contextChecked.Should().BeTrue();
    }
}
