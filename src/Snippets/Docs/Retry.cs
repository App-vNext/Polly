using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Polly.Retry;
using Polly.Timeout;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Retry
{
    public static void Usage()
    {
        #region retry

        // Add retry using the default options.
        // See https://www.pollydocs.org/strategies/retry#defaults for defaults.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions());

        // For instant retries with no delay
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            Delay = TimeSpan.Zero
        });

        // For advanced control over the retry behavior, including the number of attempts,
        // delay between retries, and the types of exceptions to handle.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,  // Adds a random factor to the delay
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(3),
        });

        // To use a custom function to generate the delay for retries
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            DelayGenerator = args =>
            {
                var delay = args.AttemptNumber switch
                {
                    0 => TimeSpan.Zero,
                    1 => TimeSpan.FromSeconds(1),
                    _ => TimeSpan.FromSeconds(5)
                };

                // This example uses a synchronous delay generator,
                // but the API also supports asynchronous implementations.
                return new ValueTask<TimeSpan?>(delay);
            }
        });

        // To extract the delay from the result object
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            DelayGenerator = args =>
            {
                if (args.Outcome.Result is HttpResponseMessage responseMessage &&
                    TryGetDelay(responseMessage, out TimeSpan delay))
                {
                    return new ValueTask<TimeSpan?>(delay);
                }

                // Returning null means the retry strategy will use its internal delay for this attempt.
                return new ValueTask<TimeSpan?>((TimeSpan?)null);
            }
        });

        // To get notifications when a retry is performed
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            OnRetry = args =>
            {
                Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

                // Event handlers can be asynchronous; here, we return an empty ValueTask.
                return default;
            }
        });

        // To keep retrying indefinitely or until success use int.MaxValue.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = int.MaxValue,
        });

        #endregion
    }

    private static bool TryGetDelay(HttpResponseMessage response, out TimeSpan delay)
    {
        if (response.Headers.TryGetValues("Retry-After", out var values) &&
            values.FirstOrDefault() is string retryAfterValue &&
            int.TryParse(retryAfterValue, CultureInfo.InvariantCulture, out int retryAfterSeconds))
        {
            delay = TimeSpan.FromSeconds(retryAfterSeconds);
            return true;
        }

        delay = TimeSpan.Zero;
        return false;
    }

    public static async Task MaxDelay()
    {
        var cancellationToken = CancellationToken.None;

        #region retry-pattern-max-delay

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = int.MaxValue,
                BackoffType = DelayBackoffType.Exponential,

                // Initially, we aim for an exponential backoff, but after a certain number of retries, we set a maximum delay of 15 minutes.
                MaxDelay = TimeSpan.FromMinutes(15),
                UseJitter = true
            })
            .Build();

        // Background processing
        while (!cancellationToken.IsCancellationRequested)
        {
            await pipeline.ExecuteAsync(async token =>
            {
                // In the event of a prolonged service outage, we can afford to wait for a successful retry since this is a background task.
                await SynchronizeDataAsync(token);
            },
            cancellationToken);

            await Task.Delay(TimeSpan.FromMinutes(30)); // The sync runs every 30 minutes.
        }

        #endregion

        static ValueTask SynchronizeDataAsync(CancellationToken cancellationToken) => default;
    }

    public static void AntiPattern_1()
    {
        #region retry-anti-pattern-1

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<BrokenCircuitException>()
                .Handle<TimeoutRejectedException>()
                .Handle<SocketException>()
                .Handle<RateLimitRejectedException>(),
                MaxRetryAttempts = 3,
            })
            .Build();

        #endregion
    }

    public static void Pattern_1()
    {
        #region retry-pattern-1

        ImmutableArray<Type> networkExceptions = new[]
        {
            typeof(SocketException),
            typeof(HttpRequestException),
        }.ToImmutableArray();

        ImmutableArray<Type> strategyExceptions = new[]
        {
            typeof(TimeoutRejectedException),
            typeof(BrokenCircuitException),
            typeof(RateLimitRejectedException),
        }.ToImmutableArray();

        ImmutableArray<Type> retryableExceptions = networkExceptions
            .Union(strategyExceptions)
            .ToImmutableArray();

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = ex => new ValueTask<bool>(retryableExceptions.Contains(ex.GetType())),
                MaxRetryAttempts = 3,
            })
            .Build();

        #endregion
    }

    public static void AntiPattern_2()
    {
        #region retry-anti-pattern-2

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = _ => ValueTask.FromResult(true),
                Delay = TimeSpan.FromHours(24),
            })
            .Build();

        #endregion
    }

    public static void AntiPattern_3()
    {
        #region retry-anti-pattern-3

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                DelayGenerator = args =>
                {
                    var delay = args.AttemptNumber switch
                    {
                        <= 5 => TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber)),
                        _ => TimeSpan.FromMinutes(3)
                    };
                    return new ValueTask<TimeSpan?>(delay);
                }
            })
            .Build();

        #endregion
    }

    public static void Pattern_3()
    {
        #region retry-pattern-3

        var slowRetries = new RetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromMinutes(3),
            BackoffType = DelayBackoffType.Constant
        };

        var quickRetries = new RetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(1),
            UseJitter = true,
            BackoffType = DelayBackoffType.Exponential
        };

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(slowRetries)
            .AddRetry(quickRetries)
            .Build();

        #endregion
    }

    public static void AntiPattern_4()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "");
        static bool IsRetryable(Uri? uri) => true;
        #region retry-anti-pattern-4

        var retry =
            IsRetryable(request.RequestUri)
                ? new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new()).Build()
                : ResiliencePipeline<HttpResponseMessage>.Empty;

        #endregion
    }

    public static void Pattern_4()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "");
        static bool IsRetryable(Uri? uri) => true;
        #region retry-pattern-4

        var retry = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new()
            {
                ShouldHandle = _ => ValueTask.FromResult(IsRetryable(request.RequestUri))
            })
            .Build();

        #endregion
    }

    public static async Task AntiPattern_5()
    {
        static void BeforeEachAttempt() => Debug.WriteLine("Before attempt");
        static ValueTask DoSomething(CancellationToken ct) => ValueTask.CompletedTask;

        #region retry-anti-pattern-5
        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                OnRetry = args =>
                {
                    BeforeEachAttempt();
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

        BeforeEachAttempt();
        await retry.ExecuteAsync(DoSomething);

        #endregion
    }

    public static async Task Pattern_5()
    {
        static void BeforeEachAttempt() => Debug.WriteLine("Before attempt");
        static ValueTask DoSomething(CancellationToken ct) => ValueTask.CompletedTask;

        #region retry-pattern-5

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new())
            .Build();

        await retry.ExecuteAsync(ct =>
        {
            BeforeEachAttempt();
            return DoSomething(ct);
        });

        #endregion
    }

    private record struct Foo;
    public static async Task AntiPattern_6()
    {
        var httpClient = new HttpClient();

        #region retry-anti-pattern-6

        var builder = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = 3
            });

        builder.AddTimeout(TimeSpan.FromMinutes(1));

        var pipeline = builder.Build();
        await pipeline.ExecuteAsync(static async (httpClient, ct) =>
        {
            var stream = await httpClient.GetStreamAsync(new Uri("endpoint"), ct);
            var foo = await JsonSerializer.DeserializeAsync<Foo>(stream, cancellationToken: ct);
        },
        httpClient);

        #endregion
    }

    public static async Task Pattern_6()
    {
        var httpClient = new HttpClient();

        #region retry-pattern-6

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = 3
            })
            .Build();

        var stream = await retry.ExecuteAsync(
            static async (httpClient, ct) =>
                await httpClient.GetStreamAsync(new Uri("endpoint"), ct),
            httpClient);

        var timeout = new ResiliencePipelineBuilder<Foo>()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .Build();

        var foo = await timeout.ExecuteAsync((ct) => JsonSerializer.DeserializeAsync<Foo>(stream, cancellationToken: ct));

        #endregion
    }

    public static void AntiPattern_7()
    {
        #region retry-anti-pattern-7

        var ctsKey = new ResiliencePropertyKey<CancellationTokenSource>("cts");
        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                OnRetry = args =>
                {
                    if (args.Outcome.Exception is TimeoutException)
                    {
                        if (args.Context.Properties.TryGetValue(ctsKey, out var cts))
                        {
                            cts.Cancel();
                        }
                    }

                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        #endregion
    }

    public static void Pattern_7()
    {
        #region retry-pattern-7

        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not TimeoutException)
            })
            .Build();

        #endregion
    }
}
