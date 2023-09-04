using Polly;
using Polly.Retry;

namespace Snippets.Core;

#pragma warning disable IDE0022 // Use expression body for method
#pragma warning disable IDE0021 // Use expression body for constructor

internal static class Snippets
{
    public static void NonGenericPipeline()
    {
        #region create-non-generic-pipeline

        ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new())
            .AddCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // test

        #endregion
    }

    public static void GenericPipeline()
    {
        #region create-generic-pipeline

        ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new())
            .AddCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        #endregion
    }

    #region on-retry-args

    public readonly struct OnRetryArguments<TResult>
    {
        public OnRetryArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber)
        {
            Context = context;
            Outcome = outcome;
            AttemptNumber = attemptNumber;
        }

        public ResilienceContext Context { get; } // Include the Context property

        public Outcome<TResult> Outcome { get; } // Includes the outcome associated with the event

        public int AttemptNumber { get; }
    }

    #endregion

    #region on-timeout-args

    public readonly struct OnTimeoutArguments
    {
        public OnTimeoutArguments(ResilienceContext context, TimeSpan timeout)
        {
            Context = context;
            Timeout = timeout;
        }

        public ResilienceContext Context { get; } // Include the Context property

        public TimeSpan Timeout { get; } // Additional event-related properties
    }

    #endregion

    #region add-my-custom-strategy

    public static TBuilder AddMyCustomStrategy<TBuilder>(this TBuilder builder, MyCustomStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        return builder.AddStrategy(context => new MyCustomStrategy(), options);
    }

    public class MyCustomStrategyOptions : ResilienceStrategyOptions
    {
        public MyCustomStrategyOptions()
        {
            Name = "MyCustomStrategy";
        }
    }

    #endregion


    #region my-custom-strategy

    internal class MyCustomStrategy : ResilienceStrategy
    {
        protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            // Perform actions before execution

            var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

            // Perform actions after execution

            return outcome;
        }
    }

    #endregion

    public static void DelegateUsage()
    {
        #region delegate-usage

        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {

                // Non-Generic predicate for multiple result types
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: string result } when result == "Failure" => PredicateResult.True(),
                    { Result: int result } when result == -1 => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
            })
            .Build();

        new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                // Generic predicate for a single result type
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: { } result } when result == "Failure" => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
            })
            .Build();

        #endregion;
    }
}
