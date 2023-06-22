using Polly;
using Polly.Retry;
using Retries;
using System.Net;

var helper = new ExecuteHelper();

// ------------------------------------------------------------------------
// 1. Create a retry strategy that only handles all exceptions
// ------------------------------------------------------------------------

ResilienceStrategy strategy = new ResilienceStrategyBuilder()
    // Default retry options handle all exceptions
    .AddRetry(new RetryStrategyOptions())
    .Build();

Console.WriteLine("---------------------------------------");
strategy.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 2. Customize the retry behavior
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried using PredicateBuilder
        ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
        RetryCount = 4,
        BaseDelay = TimeSpan.FromSeconds(1),

        // The recommended backoff type for HTTP scenarios
        // See here for more information: https://github.com/App-vNext/Polly/wiki/Retry-with-jitter#more-complex-jitter
        BackoffType = RetryBackoffType.ExponentialWithJitter
    })
    .Build();

Console.WriteLine("---------------------------------------");
strategy.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 3. Register the callbacks
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried using switch expressions
        ShouldHandle = args => args.Exception switch
        {
            InvalidOperationException => PredicateResult.True,
            _ => PredicateResult.False,
        },
        OnRetry = outcome =>
        {
            Console.WriteLine($"Retrying attempt {outcome.Arguments.Attempt}...");
            return default;
        }
    })
    .Build();

Console.WriteLine("---------------------------------------");
strategy.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 4. Create an HTTP retry strategy that handles both exceptions and results
// ------------------------------------------------------------------------

ResilienceStrategy<HttpResponseMessage> httpStrategy = new ResilienceStrategyBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        // Specify what exceptions or results should be retried
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<InvalidOperationException>()
            .HandleResult(r=>r.StatusCode == HttpStatusCode.InternalServerError),
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

Console.WriteLine("---------------------------------------");
httpStrategy.Execute(helper.ExecuteUnstable);
