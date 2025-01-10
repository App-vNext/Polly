using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void HandleMultipleResults_898()
    {
        var cancellationToken = CancellationToken.None;
        var isRetryKey = new ResiliencePropertyKey<bool>("is-retry");
        var options = new RetryStrategyOptions
        {
            BackoffType = DelayBackoffType.Constant,
            MaxRetryAttempts = 1,
            Delay = TimeSpan.FromMilliseconds(1),
            ShouldHandle = args => args.Outcome switch
            {
                // handle string results
                { Result: string res } when res == "error" => PredicateResult.True(),

                // handle int results
                { Result: int res } when res == -1 => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            OnRetry = args =>
            {
                // add a callback updates the resilience context with the retry marker
                args.Context.Properties.Set(isRetryKey, true);
                return default;
            }
        };

        // create the strategy
        var strategy = new ResiliencePipelineBuilder().AddRetry(options).Build();

        // check that int-based results is retried
        bool isRetry = false;
        strategy.Execute(_ =>
        {
            if (isRetry)
            {
                return 0;
            }

            isRetry = true;
            return -1;
        }, cancellationToken).Should().Be(0);

        // check that string-based results is retried
        isRetry = false;
        strategy.Execute(_ =>
        {
            if (isRetry)
            {
                return "no-error";
            }

            isRetry = true;
            return "error";
        }, cancellationToken).Should().Be("no-error");
    }
}
