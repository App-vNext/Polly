using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

// ----------------------------------------------------------------------------
// Create a generic resilience strategy using ResilienceStrategyBuilder<T>
// ----------------------------------------------------------------------------

// The generic ResilienceStrategyBuilder<T> creates a ResilienceStrategy<T>
// that can execute synchronous and asynchronous callbacks that return T.

ResilienceStrategy<HttpResponseMessage> strategy = new ResilienceStrategyBuilder<HttpResponseMessage>()
    .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
    {
        FallbackAction = _ =>
        {
            // Return fallback result
            return Outcome.FromResultAsTask(new HttpResponseMessage(HttpStatusCode.OK));
        },
        // You can also use switch expressions for succinct syntax
        ShouldHandle = outcome => outcome switch
        {
            // The "PredicateResult.True" is shorthand to "new ValueTask<bool>(true)"
            { Exception: HttpRequestException } => PredicateResult.True,
            { Result: HttpResponseMessage response } when response.StatusCode == HttpStatusCode.InternalServerError => PredicateResult.True,
            _ => PredicateResult.False
        },
        OnFallback = _ => { Console.WriteLine("Fallback!"); return default; }
    })
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        // You can use "PredicateBuilder" to configure the predicates
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .Handle<HttpRequestException>(),
        // Register user callback called whenever retry occurs
        OnRetry = outcome => { Console.WriteLine($"Retrying '{outcome.Result?.StatusCode}'..."); return default; },
        BaseDelay = TimeSpan.FromMilliseconds(400),
        BackoffType = RetryBackoffType.Constant,
        RetryCount = 3
    })
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromMilliseconds(500),
        // Register user callback called whenever timeout occurs
        OnTimeout = _ => { Console.WriteLine("Timeout occurred!"); return default; }
    })
    .Build();

var response = await strategy.ExecuteAsync(
    async token =>
    {
        await Task.Delay(10, token);
        // This causes the action fail, thus using the fallback strategy above
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
    },
    CancellationToken.None);

Console.WriteLine($"Response: {response.StatusCode}");
