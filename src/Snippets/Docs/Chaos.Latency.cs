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

        // 10% of invocations will be randomly affected
        var basicOptions = new LatencyStrategyOptions
        {
            Latency = TimeSpan.FromSeconds(30),
            Enabled = true,
            InjectionRate = 0.1
        };

        // To use a custom function to generate the latency to inject
        var optionsWithLatencyGenerator = new LatencyStrategyOptions
        {
            LatencyGenerator = static args =>
            {
                TimeSpan latency = args.Context.OperationKey switch
                {
                    "DataLayer" => TimeSpan.FromMilliseconds(500),
                    "ApplicationLayer" => TimeSpan.FromSeconds(2),
                    // When the latency generator returns Zero, the strategy
                    // won't inject any delay and just invokes the user's callback.
                    _ => TimeSpan.Zero
                };

                return new ValueTask<TimeSpan>(latency);
            },
            Enabled = true,
            InjectionRate = 0.1
        };

        // To get notifications when a delay is injected
        var optionsOnBehaviorInjected = new LatencyStrategyOptions
        {
            Latency = TimeSpan.FromSeconds(30),
            Enabled = true,
            InjectionRate = 0.1,
            OnLatency = static args =>
            {
                Console.WriteLine($"OnLatency, Latency: {args.Latency}, Operation: {args.Context.OperationKey}.");
                return default;
            }
        };

        // Add a latency strategy with a LatencyStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosLatency(optionsDefault);
        new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosLatency(optionsWithLatencyGenerator);

        // There are also a handy overload to inject the chaos easily
        new ResiliencePipelineBuilder().AddChaosLatency(0.1, TimeSpan.FromSeconds(30));
        #endregion

        #region chaos-latency-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .AddTimeout(TimeSpan.FromSeconds(5))
            .AddChaosLatency(new LatencyStrategyOptions // Chaos strategies are usually placed as the last ones in the pipeline
            {
                Latency = TimeSpan.FromSeconds(10),
                Enabled = true,
                InjectionRate = 0.1
            })
            .Build();
        #endregion
    }
}
