using Polly;
using Polly.Retry;
using System.Net;

// ------------------------------------------------------------------------
// 1. Create a retry strategy that only handles exceptions
// ------------------------------------------------------------------------

ResilienceStrategy strategy = new ResilienceStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried
        ShouldRetry = outcome =>
        {
            if (outcome.Exception is InvalidOperationException)
            {
                return PredicateResult.True;
            }

            return PredicateResult.False;
        },
    })
    .Build();

// ------------------------------------------------------------------------
// 2. Customize the retry behavior
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried
        ShouldRetry = outcome =>
        {
            if (outcome.Exception is InvalidOperationException)
            {
                return PredicateResult.True;
            }

            return PredicateResult.False;
        },
        RetryCount = 4,
        BaseDelay = TimeSpan.FromSeconds(1),

        // The recommended backoff type for HTTP scenarios
        BackoffType = RetryBackoffType.ExponentialWithJitter
    })
    .Build();

// ------------------------------------------------------------------------
// 3. Register to callbacks
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried
        ShouldRetry = outcome =>
        {
            if (outcome.Exception is InvalidOperationException)
            {
                return PredicateResult.True;
            }

            return PredicateResult.False;
        },

        OnRetry = outcome =>
        {
            Console.WriteLine($"Retrying attempt {outcome.Arguments.Attempt}...");
            return default;
        }
    })
    .Build();

// ------------------------------------------------------------------------
// 4. Create a HTTP retry strategy that handles both exceptions and results
// ------------------------------------------------------------------------

ResilienceStrategy<HttpResponseMessage> httpStrategy = new ResilienceStrategyBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        // Specify what exceptions or results should be retried
        ShouldRetry = outcome =>
        {
            // Now, also handle results
            if (outcome.Result?.StatusCode == HttpStatusCode.InternalServerError)
            {
                return PredicateResult.True;
            }

            if (outcome.Exception is InvalidOperationException)
            {
                return PredicateResult.True;
            }

            return PredicateResult.False;
        },

        // Specify delay generator
        RetryDelayGenerator = outcome =>
        {
            if (outcome.Result is not null && outcome.Result.Headers.TryGetValues("Retry-After", out var value))
            {
                // Return delay based on header
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(int.Parse(value.Single())));
            }

            // Return delay hinted by the retry strategy
            return new ValueTask<TimeSpan>(outcome.Arguments.DelayHint);
        }
    })
    .Build();
