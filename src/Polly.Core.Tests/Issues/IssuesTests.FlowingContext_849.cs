using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void FlowingContext_849()
    {
        var contextChecked = false;
        var retryOptions = new RetryStrategyOptions();

        // configure the predicate and use the context
        retryOptions.ShouldRetry.HandleResult<int>((_, args) =>
        {
            // access the context to evaluate the retry
            ResilienceContext context = args.Context;
            context.Should().NotBeNull();
            contextChecked = true;
            return false;
        });

        var strategy = new ResilienceStrategyBuilder().AddRetry(retryOptions).Build();

        // execute the retry
        strategy.Execute(_ => 0);

        contextChecked.Should().BeTrue();
    }
}
