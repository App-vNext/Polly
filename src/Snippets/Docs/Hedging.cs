using System.Net;
using System.Net.Http;
using Polly.Hedging;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

#pragma warning disable CA1031 // Do not catch general exception types

internal static class Hedging
{
    public static void Usage()
    {
        #region hedging

        // Hedging with default options.
        // See https://www.pollydocs.org/strategies/hedging#defaults for defaults.
        var optionsDefaults = new HedgingStrategyOptions<HttpResponseMessage>();

        // A customized hedging strategy that retries up to 3 times if the execution
        // takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
        var optionsComplex = new HedgingStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<SomeExceptionType>()
                .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
            MaxHedgedAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            ActionGenerator = static args =>
            {
                Console.WriteLine("Preparing to execute hedged action.");

                // Return a delegate function to invoke the original action with the action context.
                // Optionally, you can also create a completely new action to be executed.
                return () => args.Callback(args.ActionContext);
            }
        };

        // Subscribe to hedging events.
        var optionsOnHedging = new HedgingStrategyOptions<HttpResponseMessage>
        {
            OnHedging = static args =>
            {
                Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
                return default;
            }
        };

        // Add a hedging strategy with a HedgingStrategyOptions<TResult> instance to the pipeline
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddHedging(optionsDefaults);

        #endregion
    }

    public static void DynamicMode()
    {
        #region hedging-dynamic-mode

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new()
            {
                MaxHedgedAttempts = 3,
                DelayGenerator = args =>
                {
                    var delay = args.AttemptNumber switch
                    {
                        0 or 1 => TimeSpan.Zero, // Parallel mode
                        _ => TimeSpan.FromSeconds(-1) // switch to Fallback mode
                    };

                    return new ValueTask<TimeSpan>(delay);
                }
            });

        #endregion
    }

    public static void ActionGenerator()
    {
        var customDataKey = new ResiliencePropertyKey<string>("my-key");

        #region hedging-action-generator

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new()
            {
                ActionGenerator = args =>
                {
                    // You can access data from the original (primary) context here
                    var customData = args.PrimaryContext.Properties.GetValue(customDataKey, "default-custom-data");

                    Console.WriteLine($"Hedging, Attempt: {args.AttemptNumber}, Custom Data: {customData}");

                    // Here, we can access the original callback and return it or return a completely new action
                    var callback = args.Callback;

                    // A function that returns a ValueTask<Outcome<HttpResponseMessage>> is required.
                    return async () =>
                    {
                        try
                        {
                            // A dedicated ActionContext is provided for each hedged action.
                            // It comes with a separate CancellationToken created specifically for this hedged attempt,
                            // which can be cancelled later if needed.
                            //
                            // Note that the "MyRemoteCallAsync" call won't have any additional resilience applied.
                            // You are responsible for wrapping it with any additional resilience pipeline.
                            var response = await MyRemoteCallAsync(args.ActionContext.CancellationToken);

                            return Outcome.FromResult(response);
                        }
                        catch (Exception e)
                        {
                            // Note: All exceptions should be caught and converted to Outcome.
                            return Outcome.FromException<HttpResponseMessage>(e);
                        }
                    };
                }
            });

        #endregion
    }

    #region hedging-resilience-keys

    internal static class ResilienceKeys
    {
        public static readonly ResiliencePropertyKey<HttpRequestMessage> RequestMessage = new("MyFeature.RequestMessage");
    }

    #endregion

    #region hedging-handler

    internal sealed class HedgingHandler : DelegatingHandler
    {
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

        public HedgingHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
        {
            _pipeline = pipeline;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = ResilienceContextPool.Shared.Get(cancellationToken);

            // Store the incoming request in the context
            context.Properties.Set(ResilienceKeys.RequestMessage, request);

            try
            {
                return await _pipeline.ExecuteAsync(async cxt =>
                {
                    // Allow the pipeline to use request message that was stored in the context.
                    // This allows replacing the request message with a new one in the resilience pipeline.
                    request = cxt.Properties.GetValue(ResilienceKeys.RequestMessage, request);

                    return await base.SendAsync(request, cxt.CancellationToken);
                },
                context);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }
    }

    #endregion

    public static void ParametrizedCallback()
    {
        #region hedging-parametrized-action-generator

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new()
            {
                ActionGenerator = args =>
                {
                    if (!args.PrimaryContext.Properties.TryGetValue(ResilienceKeys.RequestMessage, out var request))
                    {
                        throw new InvalidOperationException("The request message must be provided.");
                    }

                    // Prepare a new request message for the callback, potentially involving:
                    //
                    // - Cloning the request message
                    // - Providing alternate endpoint URLs
                    request = PrepareRequest(request);

                    // Override the request message in the action context
                    args.ActionContext.Properties.Set(ResilienceKeys.RequestMessage, request);

                    // Then, execute the original callback
                    return () => args.Callback(args.ActionContext);
                }
            });

        #endregion
    }

    private static HttpRequestMessage PrepareRequest(HttpRequestMessage request) => request;

    private static ValueTask<HttpResponseMessage> MyRemoteCallAsync(CancellationToken cancellationToken)
        => new(new HttpResponseMessage());
}
