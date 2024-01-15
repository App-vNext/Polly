using System.Net;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Latency;
using Polly.Timeout;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void LatencyUsage()
    {
        #region chaos-latency-usage
        // Latency using the default options.
        // See https://www.pollydocs.org/chaos/latency#defaults for defaults.
        var optionsDefault = new LatencyStrategyOptions();

        // 60% of invocations will be randomly affected.
        var basicOptions = new LatencyStrategyOptions
        {
            Latency = TimeSpan.FromSeconds(30),
            Enabled = true,
            InjectionRate = 0.6
        };

        // To use a custom function to generate the latency to inject.
        var optionsWithLatencyGenerator = new LatencyStrategyOptions
        {
            LatencyGenerator = static args =>
            {
                TimeSpan latency = args.Context.OperationKey switch
                {
                    "DataLayer" => TimeSpan.FromMilliseconds(500),
                    "ApplicationLayer" => TimeSpan.FromSeconds(2),
                    _ => TimeSpan.Zero // When the latency generator returns Zero the strategy won't inject any delay and it will just invoke the user's callback
                };

                return new ValueTask<TimeSpan>(latency);
            },
            Enabled = true,
            InjectionRate = 0.6
        };

        // To get notifications when a delay is injected
        var optionsOnBehaviorInjected = new LatencyStrategyOptions
        {
            Latency = TimeSpan.FromSeconds(30),
            Enabled = true,
            InjectionRate = 0.6,
            OnLatency = static args =>
            {
                Console.WriteLine($"OnLatency, Latency: {args.Latency}, Operation: {args.Context.OperationKey}.");
                return default;
            }
        };

        // Add a latency strategy with a LatencyStrategyOptions{<TResult>} instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosLatency(optionsDefault);
        new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosLatency(optionsWithLatencyGenerator);

        // There are also a handy overload to inject the chaos easily.
        new ResiliencePipelineBuilder().AddChaosLatency(0.6, TimeSpan.FromSeconds(30));
        #endregion

        #region chaos-latency-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(new LatencyStrategyOptions // Monkey strategies are usually placed innermost in the pipelines
            {
                Latency = TimeSpan.FromSeconds(10),
                Enabled = true,
                InjectionRate = 0.6
            })
            .AddTimeout(TimeSpan.FromSeconds(5))
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .Build();
        #endregion
    }
}
