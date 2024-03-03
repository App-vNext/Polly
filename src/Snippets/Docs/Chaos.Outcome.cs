using System.Net;
using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Simmy.Outcomes;

namespace Snippets.Docs;

#pragma warning disable CA5394 // Do not use insecure randomness

internal static partial class Chaos
{
    public static void OutcomeUsage()
    {
        #region chaos-outcome-usage
        // To use OutcomeGenerator<T> to register the results and exceptions to be injected (equal probability)
        var optionsWithResultGenerator = new ChaosOutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests))
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError))
                .AddException(() => new HttpRequestException("Chaos request exception.")),
            InjectionRate = 0.1
        };

        // To get notifications when a result is injected
        var optionsOnBehaviorInjected = new ChaosOutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            InjectionRate = 0.1,
            OnOutcomeInjected = static args =>
            {
                Console.WriteLine($"OnBehaviorInjected, Outcome: {args.Outcome.Result}, Operation: {args.Context.OperationKey}.");
                return default;
            }
        };

        // Add a result strategy with a ChaosOutcomeStrategyOptions{<TResult>} instance to the pipeline
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosOutcome(optionsWithResultGenerator);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosOutcome(optionsOnBehaviorInjected);

        // There are also a couple of handy overloads to inject the chaos easily
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosOutcome(0.1, () => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        #endregion

        #region chaos-outcome-execution
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = static args => args.Outcome switch
                {
                    { Result.StatusCode: HttpStatusCode.InternalServerError } => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(3),
            })
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage> // Chaos strategies are usually placed as the last ones in the pipeline
            {
                OutcomeGenerator = static args =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
                },
                InjectionRate = 0.1
            })
            .Build();
        #endregion
    }

    public static void OutcomeGenerator()
    {
        #region chaos-outcome-generator-class

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
            {
                // Use OutcomeGenerator<T> to register the results and exceptions to be injected
                OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                    .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)) // Result generator
                    .AddResult(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests), weight: 50) // Result generator with weight
                    .AddResult(context => CreateResultFromContext(context)) // Access the ResilienceContext to create result
                    .AddException<HttpRequestException>(), // You can also register exceptions
            });

        #endregion
    }

    public static void OutcomeGeneratorDelegates()
    {
        #region chaos-outcome-generator-delegate

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
            {
                // The same behavior can be achieved with delegates
                OutcomeGenerator = args =>
                {
                    Outcome<HttpResponseMessage>? outcome = Random.Shared.Next(350) switch
                    {
                        < 100 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                        < 150 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)),
                        < 250 => Outcome.FromResult(CreateResultFromContext(args.Context)),
                        < 350 => Outcome.FromException<HttpResponseMessage>(new TimeoutException()),
                        _ => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.OK))
                    };

                    return ValueTask.FromResult(outcome);
                }
            });

        #endregion
    }

    public static void AntiPattern_GeneratorDelegateInjectFault()
    {
        #region chaos-outcome-anti-pattern-generator-inject-fault
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
            {
                InjectionRate = 0.5, // same injection rate for both fault and outcome
                OutcomeGenerator = static args =>
                {
                    Outcome<HttpResponseMessage>? outcome = Random.Shared.Next(350) switch
                    {
                        < 100 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                        < 150 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)),
                        < 250 => Outcome.FromResult(CreateResultFromContext(args.Context)),
                        < 350 => Outcome.FromException<HttpResponseMessage>(new HttpRequestException("Chaos request exception.")),
                        _ => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.OK))
                    };

                    return ValueTask.FromResult(outcome);
                },
                OnOutcomeInjected = static args =>
                {
                    // You might have to put some logic here to determine what kind of output was injected. 😕
                    if (args.Outcome.Exception != null)
                    {
                        Console.WriteLine($"OnBehaviorInjected, Exception: {args.Outcome.Exception.Message}, Operation: {args.Context.OperationKey}.");
                    }
                    else
                    {
                        Console.WriteLine($"OnBehaviorInjected, Outcome: {args.Outcome.Result}, Operation: {args.Context.OperationKey}.");
                    }

                    return default;
                }
            })
            .Build();

        #endregion
    }

    public static void Pattern_GeneratorDelegateInjectFaultAndOutcome()
    {
        #region chaos-outcome-pattern-generator-inject-fault
        var randomThreshold = Random.Shared.Next(350);
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = 0.1, // Different injection rate for faults
                EnabledGenerator = static args => ValueTask.FromResult(ShouldEnableFaults(args.Context)), // Different settings might apply to inject faults
                FaultGenerator = args =>
                {
                    Exception? exception = randomThreshold switch
                    {
                        >= 250 and < 350 => new HttpRequestException("Chaos request exception."),
                        _ => null
                    };

                    return new ValueTask<Exception?>(exception);
                },
                OnFaultInjected = static args =>
                {
                    Console.WriteLine($"OnFaultInjected, Exception: {args.Fault.Message}, Operation: {args.Context.OperationKey}.");
                    return default;
                }
            })
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
            {
                InjectionRate = 0.5, // Different injection rate for outcomes
                EnabledGenerator = static args => ValueTask.FromResult(ShouldEnableOutcome(args.Context)), // Different settings might apply to inject outcomes
                OutcomeGenerator = args =>
                {
                    Outcome<HttpResponseMessage>? outcome = randomThreshold switch
                    {
                        < 100 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                        < 150 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)),
                        < 250 => Outcome.FromResult(CreateResultFromContext(args.Context)),
                        _ => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.OK))
                    };

                    return ValueTask.FromResult(outcome);
                },
                OnOutcomeInjected = static args =>
                {
                    Console.WriteLine($"OnBehaviorInjected, Outcome: {args.Outcome.Result}, Operation: {args.Context.OperationKey}.");
                    return default;
                }
            })
            .Build();
        #endregion
    }

    public static void AntiPattern_OnlyInjectFault()
    {
        #region chaos-outcome-anti-pattern-only-inject-fault

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
            {
                OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                    .AddException<HttpRequestException>(),
            });

        #endregion
    }

    public static void Pattern_OnlyInjectFault()
    {
        #region chaos-outcome-pattern-only-inject-fault

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                FaultGenerator = new FaultGenerator()
                    .AddException<HttpRequestException>(),
            });

        #endregion
    }

    private static bool ShouldEnableFaults(ResilienceContext context) => true;

    private static bool ShouldEnableOutcome(ResilienceContext context) => true;

    private static HttpResponseMessage CreateResultFromContext(ResilienceContext context) => new(HttpStatusCode.TooManyRequests);
}
