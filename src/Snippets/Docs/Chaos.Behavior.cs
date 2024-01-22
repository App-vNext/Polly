using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Behavior;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void BehaviorUsage()
    {
        static ValueTask RestartRedisVM(CancellationToken cancellationToken) => ValueTask.CompletedTask;

        #region chaos-behavior-usage
        // To use a custom delegated for injected behavior
        var optionsWithBehaviorGenerator = new BehaviorStrategyOptions
        {
            BehaviorAction = static args => RestartRedisVM(args.Context.CancellationToken),
            Enabled = true,
            InjectionRate = 0.05
        };

        // To get notifications when a behavior is injected
        var optionsOnBehaviorInjected = new BehaviorStrategyOptions
        {
            BehaviorAction = static args => RestartRedisVM(args.Context.CancellationToken),
            Enabled = true,
            InjectionRate = 0.05,
            OnBehaviorInjected = static args =>
            {
                Console.WriteLine("OnBehaviorInjected, Operation: {0}.", args.Context.OperationKey);
                return default;
            }
        };

        // Add a behavior strategy with a BehaviorStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosBehavior(optionsWithBehaviorGenerator);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosBehavior(optionsOnBehaviorInjected);

        // There are also a handy overload to inject the chaos easily
        new ResiliencePipelineBuilder().AddChaosBehavior(0.05, RestartRedisVM);
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
            .AddChaosBehavior(new BehaviorStrategyOptions // Chaos strategies are usually placed as the last ones in the pipeline
            {
                BehaviorAction = static args => RestartRedisVM(args.Context.CancellationToken),
                Enabled = true,
                InjectionRate = 0.05
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
