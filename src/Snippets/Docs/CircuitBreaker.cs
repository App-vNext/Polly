using System.Net;
using System.Net.Http;
using Polly.CircuitBreaker;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class CircuitBreaker
{
    public static async Task Usage()
    {
        #region circuit-breaker

        // Add circuit breaker with default options.
        // See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
        new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions());

        // Add circuit breaker with customized options:
        //
        // The circuit will break if more than 50% of actions result in handled exceptions,
        // within any 10-second sampling duration, and at least 8 actions are processed.
        new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
        });

        // Adds a circuit breaker with dynamic break duration:
        //
        // Same circuit breaking conditions as above, but with a dynamic break duration based on the failure count.
        // The duration is calculated as: minimum of (20 + 2^failureCount) seconds and capped at 400 seconds.
        // The specified BreakDuration = TimeSpan.FromSeconds(30) will not be used due to the dynamic BreakDurationGenerator.
        new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30),
            BreakDurationGenerator = static args => TimeSpan.FromSeconds(Math.Min(20 + Math.Pow(2, args.FailureCount), 400)),
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
        });

        // Handle specific failed results for HttpResponseMessage:
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
            });

        // Monitor the circuit state, useful for health reporting:
        var stateProvider = new CircuitBreakerStateProvider();

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddCircuitBreaker(new() { StateProvider = stateProvider })
            .Build();

        /*
        CircuitState.Closed - Normal operation; actions are executed.
        CircuitState.Open - Circuit is open; actions are blocked.
        CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
        CircuitState.Isolated - Circuit is manually held open; actions are blocked.
        */

        // Manually control the Circuit Breaker state:
        var manualControl = new CircuitBreakerManualControl();

        new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() { ManualControl = manualControl })
            .Build();

        // Manually isolate a circuit, e.g., to isolate a downstream service.
        await manualControl.IsolateAsync();

        // Manually close the circuit to allow actions to be executed again.
        await manualControl.CloseAsync();

        #endregion
    }

    public static void AntiPattern_1()
    {
        #region circuit-breaker-anti-pattern-1
        var stateProvider = new CircuitBreakerStateProvider();
        var circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = TimeSpan.FromSeconds(5),
                StateProvider = stateProvider
            })
            .Build();

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<BrokenCircuitException>(),
                DelayGenerator = args =>
                {
                    TimeSpan? delay = TimeSpan.FromSeconds(1);
                    if (stateProvider.CircuitState == CircuitState.Open)
                    {
                        delay = TimeSpan.FromSeconds(5);
                    }

                    return ValueTask.FromResult(delay);
                }
            })
            .Build();

        #endregion
    }

    private static readonly ResiliencePropertyKey<TimeSpan?> SleepDurationKey = new("sleep_duration");
    public static void Pattern_1()
    {
        #region circuit-breaker-pattern-1
        var circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = TimeSpan.FromSeconds(5),
                OnOpened = static args =>
                {
                    args.Context.Properties.Set(SleepDurationKey, args.BreakDuration);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    args.Context.Properties.Set(SleepDurationKey, null);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<BrokenCircuitException>(),
                DelayGenerator = static args =>
                {
                    _ = args.Context.Properties.TryGetValue(SleepDurationKey, out var delay);
                    delay ??= TimeSpan.FromSeconds(1);
                    return ValueTask.FromResult(delay);
                }
            })
            .Build();

        #endregion
    }

    public static void AntiPattern_2()
    {
        #region circuit-breaker-anti-pattern-2
        static IEnumerable<TimeSpan> GetSleepDuration()
        {
            for (int i = 1; i < 10; i++)
            {
                yield return TimeSpan.FromSeconds(i);
            }
        }

        var sleepDurationProvider = GetSleepDuration().GetEnumerator();
        sleepDurationProvider.MoveNext();

        var circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = TimeSpan.FromSeconds(0.5),
                OnOpened = async args =>
                {
                    await Task.Delay(sleepDurationProvider.Current);
                    sleepDurationProvider.MoveNext();
                }

            })
            .Build();

        #endregion

        #region circuit-breaker-anti-pattern-2-ext

        circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = sleepDurationProvider.Current,
                OnOpened = async args =>
                {
                    Console.WriteLine($"Break: {sleepDurationProvider.Current}");
                    sleepDurationProvider.MoveNext();
                }

            })
            .Build();

        #endregion
    }

    public static async ValueTask AntiPattern_3()
    {
        static ValueTask CallXYZOnDownstream1(CancellationToken ct) => ValueTask.CompletedTask;
        static ResiliencePipeline GetCircuitBreaker() => ResiliencePipeline.Empty;

        #region circuit-breaker-anti-pattern-3
        // Defined in a common place
        var uriToCbMappings = new Dictionary<Uri, ResiliencePipeline>
        {
            [new Uri("https://downstream1.com")] = GetCircuitBreaker(),
            // ...
            [new Uri("https://downstreamN.com")] = GetCircuitBreaker()
        };

        // Used in the downstream 1 client
        var downstream1Uri = new Uri("https://downstream1.com");
        await uriToCbMappings[downstream1Uri].ExecuteAsync(CallXYZOnDownstream1, CancellationToken.None);
        #endregion
    }
}
