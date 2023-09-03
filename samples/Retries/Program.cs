using System.Net;
using Polly;
using Polly.Retry;
using Retries;

var helper = new ExecuteHelper();

// ------------------------------------------------------------------------
// 1. Create a retry pipeline that handles all exceptions
// ------------------------------------------------------------------------

ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    // Default retry options handle all exceptions
    .AddRetry(new RetryStrategyOptions())
    .Build();

Console.WriteLine("---------------------------------------");
pipeline.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 2. Customize the retry behavior
// ------------------------------------------------------------------------

pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried using PredicateBuilder
        ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(1),

        // The recommended backoff type for HTTP scenarios
        // See here for more information: https://github.com/App-vNext/Polly/wiki/Retry-with-jitter#more-complex-jitter
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    })
    .Build();

Console.WriteLine("---------------------------------------");
pipeline.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 3. Register the callbacks
// ------------------------------------------------------------------------

pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Specify what exceptions should be retried using switch expressions
        ShouldHandle = args => args.Outcome.Exception switch
        {
            InvalidOperationException => PredicateResult.True(),
            _ => PredicateResult.False(),
        },
        OnRetry = outcome =>
        {
            Console.WriteLine($"Retrying attempt {outcome.AttemptNumber}...");
            return default;
        }
    })
    .Build();

Console.WriteLine("---------------------------------------");
pipeline.Execute(helper.ExecuteUnstable);

// ------------------------------------------------------------------------
// 4. Create an HTTP retry pipeline that handles both exceptions and results
// ------------------------------------------------------------------------

ResiliencePipeline<HttpResponseMessage> httpPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        // Specify what exceptions or results should be retried
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<InvalidOperationException>()
            .HandleResult(r=>r.StatusCode == HttpStatusCode.InternalServerError),
        // Specify delay generator
        DelayGenerator = arguments =>
        {
            if (arguments.Outcome.Result is not null &&
                arguments.Outcome.Result.Headers.TryGetValues("Retry-After", out var value))
            {
                // Return delay based on header
                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(int.Parse(value.Single())));
            }

            // Return delay hinted by the retry strategy
            return new ValueTask<TimeSpan?>(default(TimeSpan?));
        }
    })
    .Build();

Console.WriteLine("---------------------------------------");
httpPipeline.Execute(helper.ExecuteUnstable);
