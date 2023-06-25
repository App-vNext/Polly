using Polly;
using Polly.Retry;
using Polly.Timeout;

// ------------------------------------------------------------------------
// 1. Create a simple resilience strategy using ResilienceStrategyBuilder
// ------------------------------------------------------------------------

// The ResilienceStrategyBuilder creates a ResilienceStrategy
// that can be executed synchronously or asynchronously
// and for both void and result-returning user-callbacks.
ResilienceStrategy strategy = new ResilienceStrategyBuilder()
    // Use convenience extension that accepts TimeSpan
    .AddTimeout(TimeSpan.FromSeconds(5)) 
    .Build();

// ------------------------------------------------------------------------
// 2. Execute the strategy
// ------------------------------------------------------------------------

// Synchronously
strategy.Execute(() => { });

// Asynchronously
await strategy.ExecuteAsync(async token => { await Task.Delay(10, token); }, CancellationToken.None);

// Synchronously with result
strategy.Execute(token => "some-result");

// Asynchronously with result
await strategy.ExecuteAsync(async token => { await Task.Delay(10, token); return "some-result"; }, CancellationToken.None);

// Use state to avoid lambda allocation
strategy.Execute(static state => state, "my-state");

// ------------------------------------------------------------------------
// 3. Create and execute a pipeline of strategies
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    // Add retries using the options
    .AddRetry(new RetryStrategyOptions
    {
        // To configure the predicate you can use switch expressions
        ShouldHandle = args => args.Exception switch
        {
            TimeoutRejectedException => PredicateResult.True,

            // The "PredicateResult.False" is just shorthand for "new ValueTask<bool>(true)"
            // You can also use "new PredicateBuilder().Handle<TimeoutRejectedException>()"
            _ => PredicateResult.False
        },
        // Register user callback called whenever retry occurs
        OnRetry = args => { Console.WriteLine($"Retrying...{args.Arguments.Attempt} attempt"); return default; },
        BaseDelay = TimeSpan.FromMilliseconds(400),
        BackoffType = RetryBackoffType.Constant,
        RetryCount = 3
    })
    // Add timeout using the options
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromMilliseconds(500),
        // Register user callback called whenever timeout occurs
        OnTimeout = args =>
        {
            Console.WriteLine($"Timeout occurred after {args.Timeout}!");
            return default;
        }
    })
    .Build();

try
{
    await strategy.ExecuteAsync(async token => await Task.Delay(TimeSpan.FromSeconds(2), token), CancellationToken.None);
}
catch (TimeoutRejectedException)
{
    // ok, expected
}
