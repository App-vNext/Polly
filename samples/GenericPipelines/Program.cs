#pragma warning disable SA1633 // File should have header
using System.Net;
#pragma warning restore SA1633 // File should have header
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

// ----------------------------------------------------------------------------
// Create a generic resilience pipeline using ResiliencePipelineBuilder<T>
// ----------------------------------------------------------------------------

// The generic ResiliencePipelineBuilder<T> creates a ResiliencePipeline<T>
// that can execute synchronous and asynchronous callbacks that return T.
ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
    {
        FallbackAction = _ =>
        {
            // Return fallback result
            return Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.OK));
        },

        // You can also use switch expressions for succinct syntax
        ShouldHandle = arguments => arguments.Outcome switch
        {
            // The "PredicateResult.True" is shorthand to "new ValueTask<bool>(true)"
            { Exception: HttpRequestException } => PredicateResult.True(),
            { Result: HttpResponseMessage response } when response.StatusCode == HttpStatusCode.InternalServerError => PredicateResult.True(),
            _ => PredicateResult.False(),
        },
        OnFallback = _ =>
        {
            Console.WriteLine("Fallback!");
            return default;
        },
    })
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        // You can use "PredicateBuilder" to configure the predicates
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .Handle<HttpRequestException>(),

        // Register user callback called whenever retry occurs
        OnRetry = arguments =>
        {
            Console.WriteLine($"Retrying '{arguments.Outcome.Result?.StatusCode}'...");
            return default;
        },
        Delay = TimeSpan.FromMilliseconds(400),
        BackoffType = DelayBackoffType.Constant,
        MaxRetryAttempts = 3,
    })
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(1),

        // Register user callback called whenever timeout occurs
        OnTimeout = _ =>
        {
            Console.WriteLine("Timeout occurred!");
            return default;
        },
    })
    .Build();

var response = await pipeline.ExecuteAsync(
    async token =>
    {
        await Task.Delay(10, token);

        // This causes the action fail, thus using the fallback strategy above
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
    },
    CancellationToken.None);

Console.WriteLine($"Response: {response.StatusCode}");
