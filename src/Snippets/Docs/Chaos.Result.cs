using System.Net;
using System.Net.Http;
using Polly.Retry;
using Polly.Simmy;
using Polly.Simmy.Outcomes;

namespace Snippets.Docs;

internal static partial class Chaos
{
    public static void ResultUsage()
    {
        #region chaos-result-usage
        // To use a custom function to generate the result to inject.
        var optionsWithResultGenerator = new OutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = static args =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
            },
            Enabled = true,
            InjectionRate = 0.1
        };

        // To get notifications when a result is injected
        var optionsOnBehaviorInjected = new OutcomeStrategyOptions<HttpResponseMessage>
        {
            OutcomeGenerator = static args =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
            },
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

        // There are also a couple of handy overloads to inject the chaos easily.
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
}
