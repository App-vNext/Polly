using Polly.Retry;

namespace Snippets.Docs;

internal static class Extensibility
{
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
