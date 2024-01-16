using System.Net;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Fault;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void FaultUsage()
    {
        #region chaos-fault-usage
        // Fault using the default options.
        // See https://www.pollydocs.org/chaos/fault#defaults for defaults.
        var optionsDefault = new FaultStrategyOptions();

        // 10% of invocations will be randomly affected.
        var basicOptions = new FaultStrategyOptions
        {
            Fault = new InvalidOperationException("Dummy exception"),
            Enabled = true,
            InjectionRate = 0.1
        };

        // To use a custom function to generate the fault to inject.
        var optionsWithFaultGenerator = new FaultStrategyOptions
        {
            FaultGenerator = static args =>
            {
                Exception? exception = args.Context.OperationKey switch
                {
                    "DataLayer" => new TimeoutException(),
                    "ApplicationLayer" => new InvalidOperationException(),
                    _ => null // When the fault generator returns null the strategy won't inject any fault and it will just invoke the user's callback
                };

                return new ValueTask<Exception?>(exception);
            },
            Enabled = true,
            InjectionRate = 0.1
        };

        // To get notifications when a fault is injected
        var optionsOnFaultInjected = new FaultStrategyOptions
        {
            Fault = new InvalidOperationException("Dummy exception"),
            Enabled = true,
            InjectionRate = 0.1,
            OnFaultInjected = static args =>
            {
                Console.WriteLine("OnFaultInjected, Exception: {0}, Operation: {1}.", args.Fault.Message, args.Context.OperationKey);
                return default;
            }
        };

        // Add a fault strategy with a FaultStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosFault(optionsDefault);
        new ResiliencePipelineBuilder<HttpStatusCode>().AddChaosFault(optionsWithFaultGenerator);

        // There are also a couple of handy overloads to inject the chaos easily.
        new ResiliencePipelineBuilder().AddChaosFault(0.1, () => new InvalidOperationException("Dummy exception"));
        #endregion

        #region chaos-fault-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(new FaultStrategyOptions // Monkey strategies are usually placed innermost in the pipelines
            {
                Fault = new InvalidOperationException("Dummy exception"),
                Enabled = true,
                InjectionRate = 0.1
            })
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .Build();
        #endregion
    }
}
