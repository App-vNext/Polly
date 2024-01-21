using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Fault;

namespace Snippets.Docs;

#pragma warning disable CA5394 // Do not use insecure randomness

internal static partial class Chaos
{
    public static void FaultUsage()
    {
        #region chaos-fault-usage
        // 10% of invocations will be randomly affected and one of the exceptions will be thrown (equal probability).
        var optionsBasic = new FaultStrategyOptions
        {
            FaultGenerator = new FaultGenerator()
                .AddException<InvalidOperationException>() // Uses default constructor
                .AddException(() => new TimeoutException("Chaos timeout injected.")), // Custom exception generator
            Enabled = true,
            InjectionRate = 0.1
        };

        // To use a custom delegate to generate the fault to be injected
        var optionsWithFaultGenerator = new FaultStrategyOptions
        {
            FaultGenerator = static args =>
            {
                Exception? exception = args.Context.OperationKey switch
                {
                    "DataLayer" => new TimeoutException(),
                    "ApplicationLayer" => new InvalidOperationException(),
                    // When the fault generator returns null, the strategy won't inject
                    // any fault and just invokes the user's callback.
                    _ => null
                };

                return new ValueTask<Exception?>(exception);
            },
            Enabled = true,
            InjectionRate = 0.1
        };

        // To get notifications when a fault is injected
        var optionsOnFaultInjected = new FaultStrategyOptions
        {
            FaultGenerator = new FaultGenerator().AddException<InvalidOperationException>(),
            Enabled = true,
            InjectionRate = 0.1,
            OnFaultInjected = static args =>
            {
                Console.WriteLine("OnFaultInjected, Exception: {0}, Operation: {1}.", args.Fault.Message, args.Context.OperationKey);
                return default;
            }
        };

        // Add a fault strategy with a FaultStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddChaosFault(optionsBasic);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosFault(optionsWithFaultGenerator);

        // There are also a couple of handy overloads to inject the chaos easily
        new ResiliencePipelineBuilder().AddChaosFault(0.1, () => new InvalidOperationException("Dummy exception"));
        #endregion

        #region chaos-fault-execution
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .AddChaosFault(new FaultStrategyOptions // Chaos strategies are usually placed as the last ones in the pipeline
            {
                FaultGenerator = static args => new ValueTask<Exception?>(new InvalidOperationException("Dummy exception")),
                Enabled = true,
                InjectionRate = 0.1
            })
            .Build();
        #endregion
    }

    public static void FaultGenerator()
    {
        #region chaos-fault-generator-class

        new ResiliencePipelineBuilder()
            .AddChaosFault(new FaultStrategyOptions
            {
                // Use FaultGenerator to register exceptions to be injected
                FaultGenerator = new FaultGenerator()
                    .AddException<InvalidOperationException>() // Uses default constructor
                    .AddException(() => new TimeoutException("Chaos timeout injected.")) // Custom exception generator
                    .AddException(context => CreateExceptionFromContext(context)) // Access the ResilienceContext
                    .AddException<TimeoutException>(weight: 50), // Assign weight to the exception, default is 100
            });

        #endregion
    }

    public static void FaultGeneratorDelegates()
    {
        #region chaos-fault-generator-delegate

        new ResiliencePipelineBuilder()
            .AddChaosFault(new FaultStrategyOptions
            {
                // The same behavior can be achieved with delegates
                FaultGenerator = args =>
                {
                    Exception? exception = Random.Shared.Next(350) switch
                    {
                        < 100 => new InvalidOperationException(),
                        < 200 => new TimeoutException("Chaos timeout injected."),
                        < 300 => CreateExceptionFromContext(args.Context),
                        < 350 => new TimeoutException(),
                        _ => null
                    };

                    return new ValueTask<Exception?>(exception);
                }
            });

        #endregion
    }

    private static Exception CreateExceptionFromContext(ResilienceContext context) => new InvalidOperationException();
}
