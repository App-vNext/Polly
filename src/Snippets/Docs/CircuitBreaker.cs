using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly.CircuitBreaker;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class CircuitBreaker
{
    public static async Task Usage()
    {
        #region circuit-breaker

        // Circuit breaker with default options.
        // See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
        var optionsDefaults = new CircuitBreakerStrategyOptions();

        // Circuit breaker with customized options:
        // The circuit will break if more than 50% of actions result in handled exceptions,
        // within any 10-second sampling duration, and at least 8 actions are processed.
        var optionsComplex = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
        };

        // Circuit breaker using BreakDurationGenerator:
        // The break duration is dynamically determined based on the properties of BreakDurationGeneratorArguments.
        var optionsBreakDurationGenerator = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDurationGenerator = static args => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(args.FailureCount)),
        };

        // Handle specific failed results for HttpResponseMessage:
        var optionsShouldHandle = new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<SomeExceptionType>()
                .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
        };

        // Monitor the circuit state, useful for health reporting:
        var stateProvider = new CircuitBreakerStateProvider();
        var optionsStateProvider = new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            StateProvider = stateProvider
        };

        var circuitState = stateProvider.CircuitState;

        /*
        CircuitState.Closed - Normal operation; actions are executed.
        CircuitState.Open - Circuit is open; actions are blocked.
        CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
        CircuitState.Isolated - Circuit is manually held open; actions are blocked.
        */

        // Manually control the Circuit Breaker state:
        var manualControl = new CircuitBreakerManualControl();
        var optionsManualControl = new CircuitBreakerStrategyOptions
        {
            ManualControl = manualControl
        };

        // Manually isolate a circuit, e.g., to isolate a downstream service.
        await manualControl.IsolateAsync();

        // Manually close the circuit to allow actions to be executed again.
        await manualControl.CloseAsync();

        // Add a circuit breaker strategy with a CircuitBreakerStrategyOptions{<TResult>} instance to the pipeline
        new ResiliencePipelineBuilder().AddCircuitBreaker(optionsDefaults);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddCircuitBreaker(optionsStateProvider);

        #endregion

        #region circuit-breaker-failure-handling
        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.1,
                SamplingDuration = TimeSpan.FromSeconds(1),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
            })
            .Build();

        for (int i = 0; i < 10; i++)
        {
            try
            {
                pipeline.Execute(() => throw new SomeExceptionType());
            }
            catch (SomeExceptionType)
            {
                Console.WriteLine("Operation failed please try again.");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Operation failed too many times please try again later.");
            }
        }
        #endregion
    }

    public static void AntiPattern_CircuitAwareRetry()
    {
        #region circuit-breaker-anti-pattern-circuit-aware-retry
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
    public static void Pattern_CircuitAwareRetry()
    {
        #region circuit-breaker-pattern-circuit-aware-retry
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

    public static async ValueTask AntiPattern_CircuitPerEndpoint()
    {
        static ValueTask CallXYZOnDownstream1(CancellationToken ct) => ValueTask.CompletedTask;
        static ResiliencePipeline GetCircuitBreaker() => ResiliencePipeline.Empty;

        #region circuit-breaker-anti-pattern-cb-per-endpoint
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

    public static async ValueTask Pattern_CircuitPerEndpoint()
    {
        var services = new ServiceCollection();

        #region circuit-breaker-pattern-cb-per-endpoint

        services
          .AddHttpClient("my-client")
          .AddResilienceHandler("circuit-breaker", builder =>
          {
              builder.AddCircuitBreaker(new());
          })
          .SelectPipelineByAuthority(); // This call ensures that circuit breaker is cached by each URL authority

        #endregion

        IHttpClientFactory httpClientFactory = null!;

        #region circuit-breaker-pattern-cb-per-endpoint-usage

        HttpClient client = httpClientFactory.CreateClient("my-client");

        await client.GetAsync(new Uri("https://downstream1.com/some-path"));

        #endregion
    }

    private static ValueTask<HttpResponseMessage> IssueRequest() => ValueTask.FromResult(new HttpResponseMessage());
    public static async ValueTask AntiPattern_ReduceThrownExceptions()
    {
        #region circuit-breaker-anti-pattern-reduce-thrown-exceptions

        var stateProvider = new CircuitBreakerStateProvider();
        var circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = TimeSpan.FromSeconds(0.5),
                StateProvider = stateProvider
            })
            .Build();

        if (stateProvider.CircuitState
            is not CircuitState.Open
            and not CircuitState.Isolated)
        {
            var response = await circuitBreaker.ExecuteAsync(static async ct =>
            {
                return await IssueRequest();
            }, CancellationToken.None);

            // Your code goes here to process response
        }

        #endregion
    }

    public static async ValueTask Pattern_ReduceThrownExceptions()
    {
        #region circuit-breaker-pattern-reduce-thrown-exceptions

        var context = ResilienceContextPool.Shared.Get();

        var circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BreakDuration = TimeSpan.FromSeconds(0.5),
            })
            .Build();

        Outcome<HttpResponseMessage> outcome =
            await circuitBreaker.ExecuteOutcomeAsync<HttpResponseMessage, string>(
                static async (ctx, state) =>
                {
                    try
                    {
                        var response = await IssueRequest();
                        return Outcome.FromResult(response);
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<HttpResponseMessage>(e);
                    }
                },
                context,
                "state");

        ResilienceContextPool.Shared.Return(context);

        if (outcome.Exception is BrokenCircuitException)
        {
            // The execution was stopped by the circuit breaker
        }
        else
        {
            HttpResponseMessage response = outcome.Result!;
            // Your code goes here to process the response
        }

        #endregion
    }
}
