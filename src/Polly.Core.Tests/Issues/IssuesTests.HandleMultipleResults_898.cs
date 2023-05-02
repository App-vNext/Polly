using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void HandleMultipleResults_898()
    {
        var isRetryKey = new ResiliencePropertyKey<bool>("is-retry");
        var options = new RetryStrategyOptions
        {
            BackoffType = RetryBackoffType.Constant,
            RetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        };

        // now add a callback updates the resilience context with the retry marker
        options.OnRetry.Register((_, args) => args.Context.Properties.Set(isRetryKey, true));

        // handle int results
        options.ShouldRetry.HandleResult(-1);

        // handle string results
        options.ShouldRetry.HandleResult("error");

        // create the strategy
        var strategy = new ResilienceStrategyBuilder { TimeProvider = TimeProvider.Object }.AddRetry(options).Build();

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
        }).Should().Be(0);

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
        }).Should().Be("no-error");
    }
}
