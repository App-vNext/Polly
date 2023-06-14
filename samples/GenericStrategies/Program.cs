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
        FallbackAction = async _ =>
        {
            await Task.Yield();

            // return fallback result
            return new Outcome<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        },
        // You can also use switch expressions for succinct syntax
        ShouldHandle = outcome => outcome switch
        {
            { Exception: HttpRequestException } => PredicateResult.True,
            { Result: HttpResponseMessage response } when response.StatusCode == HttpStatusCode.InternalServerError => PredicateResult.True,
            _ => PredicateResult.False
        },
        OnFallback = _ => { Console.WriteLine("Fallback!"); return default; }
    })
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldRetry = outcome =>
        {
            // We can handle specific result
            if (outcome.Result?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                // The "PredicateResult.True" is shorthand to "new ValueTask<bool>(true)"
                return PredicateResult.True;
            }

            // Or exception
            if ( outcome.Exception is HttpRequestException)
            {
                return PredicateResult.True;
            }

            return PredicateResult.False;
        },
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
        await Task.Yield();
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
    },
    CancellationToken.None);

Console.WriteLine($"Response: {response.StatusCode}");
