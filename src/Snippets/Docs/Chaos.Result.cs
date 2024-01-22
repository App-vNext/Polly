using System.Net;
using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Outcomes;

namespace Snippets.Docs;

#pragma warning disable CA5394 // Do not use insecure randomness

internal static partial class Chaos
{
    public static void ResultUsage()
    {
        #region chaos-result-usage
        // To use OutcomeGenerator<T> to register the results and exceptions to be injected (equal probability)
        var optionsWithResultGenerator = new OutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests))
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError))
                .AddException(() => new HttpRequestException("Chaos request exception.")),
            Enabled = true,
            InjectionRate = 0.1
        };

        // To get notifications when a result is injected
        var optionsOnBehaviorInjected = new OutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
                .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            Enabled = true,
            InjectionRate = 0.1,
            OnOutcomeInjected = static args =>
            {
                Console.WriteLine($"OnBehaviorInjected, Outcome: {args.Outcome.Result}, Operation: {args.Context.OperationKey}.");
                return default;
            }
        };

        // Add a result strategy with a OutcomeStrategyOptions{<TResult>} instance to the pipeline
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(optionsWithResultGenerator);
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(optionsOnBehaviorInjected);

        // There are also a couple of handy overloads to inject the chaos easily
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(0.1, () => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        #endregion

        #region chaos-result-execution
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
            .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage> // Chaos strategies are usually placed as the last ones in the pipeline
            {
                OutcomeGenerator = static args =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
                },
                Enabled = true,
                InjectionRate = 0.1
            })
            .Build();
        #endregion
    }

    public static void OutcomeGenerator()
    {
        #region chaos-outcome-generator-class

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage>
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
            .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage>
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
                        _ => null
                    };

                    return ValueTask.FromResult(outcome);
                }
            });

        #endregion
    }

    private static HttpResponseMessage CreateResultFromContext(ResilienceContext context) => new(HttpStatusCode.TooManyRequests);
}
