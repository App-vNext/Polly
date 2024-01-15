using System.Net;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Behavior;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void BehaviorUsage()
    {
        static ValueTask RestartRedisVM() => ValueTask.CompletedTask;

        #region chaos-behavior-usage
        // Behavior using the default options.
        // See https://www.pollydocs.org/chaos/behavior#defaults for defaults.
        var optionsDefault = new BehaviorStrategyOptions();

        // To use a custom function to generate the behavior to inject.
        var optionsWithBehaviorGenerator = new BehaviorStrategyOptions
        {
            BehaviorAction = static args => RestartRedisVM(),
            Enabled = true,
            InjectionRate = 0.6
        };

        // To get notifications when a behavior is injected
        var optionsOnBehaviorInjected = new BehaviorStrategyOptions
        {
            BehaviorAction = static args => RestartRedisVM(),
            Enabled = true,
            InjectionRate = 0.6,
            OnBehaviorInjected = static args =>
            {
                Console.WriteLine("OnBehaviorInjected, Operation: {0}.", args.Context.OperationKey);
                return default;
            }
        };

        // Add a behavior strategy with a BehaviorStrategyOptions{<TResult>} instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosBehavior(optionsDefault);
        new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosBehavior(optionsWithBehaviorGenerator);

        // There are also a handy overload to inject the chaos easily.
        new ResiliencePipelineBuilder().AddChaosBehavior(0.6, RestartRedisVM);
        #endregion

        #region chaos-behavior-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosBehavior(new BehaviorStrategyOptions // Monkey strategies are usually placed innermost in the pipelines
            {
                BehaviorAction = static args => RestartRedisVM(),
                Enabled = true,
                InjectionRate = 0.6
            })
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
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
