using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Behavior;
using Polly.Simmy.Latency;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void BehaviorUsage()
    {
        #region chaos-behavior-usage
        // To use a custom delegated for injected behavior
        var optionsWithBehaviorGenerator = new ChaosBehaviorStrategyOptions
        {
            BehaviorGenerator = static args => RestartRedisAsync(args.Context.CancellationToken),
            InjectionRate = 0.05
        };

        // To get notifications when a behavior is injected
        var optionsOnBehaviorInjected = new ChaosBehaviorStrategyOptions
        {
            BehaviorGenerator = static args => RestartRedisAsync(args.Context.CancellationToken),
            InjectionRate = 0.05,
            OnBehaviorInjected = static args =>
            {
                Console.WriteLine("OnBehaviorInjected, Operation: {0}.", args.Context.OperationKey);
                return default;
            }
        };

        // Add a behavior strategy with a ChaosBehaviorStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosBehavior(optionsWithBehaviorGenerator);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosBehavior(optionsOnBehaviorInjected);

        // There are also a handy overload to inject the chaos easily
        new ResiliencePipelineBuilder().AddChaosBehavior(0.05, RestartRedisAsync);
        #endregion

        #region chaos-behavior-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .AddChaosBehavior(new ChaosBehaviorStrategyOptions // Chaos strategies are usually placed as the last ones in the pipeline
            {
                BehaviorGenerator = static args => RestartRedisAsync(args.Context.CancellationToken),
                InjectionRate = 0.05
            })
            .Build();
        #endregion
    }

    public static void AntiPattern_InjectDelay()
    {
        #region chaos-behavior-anti-pattern-inject-delay
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosBehavior(new ChaosBehaviorStrategyOptions
            {
                BehaviorGenerator = static async args =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(7), args.Context.CancellationToken);
                }
            })
            .Build();

        #endregion
    }

    public static void Pattern_InjectDelay()
    {
        #region chaos-behavior-pattern-inject-delay
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                Latency = TimeSpan.FromSeconds(7),
            })
            .Build();
        #endregion
    }
}

internal class RedisConnectionException : Exception
{
    public RedisConnectionException()
    {
    }

    public RedisConnectionException(string message)
        : base(message)
    {
    }

    public RedisConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
