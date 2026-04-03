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
#if UNION_TYPES
                    Exception ex when ex is InvalidOperationException => PredicateResult.True(),
                    object result when result is string s && s == "Failure" => PredicateResult.True(),
                    object result when result is int i && i == -1 => PredicateResult.True(),
#else
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: string result } when result is "Failure" => PredicateResult.True(),
                    { Result: int result } when result is -1 => PredicateResult.True(),
#endif
                    _ => PredicateResult.False(),
                },
            })
            .Build();

        new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                // Generic predicate for a single result type
                ShouldHandle = args => args.Outcome switch
                {
#if UNION_TYPES
                    string value when value == "Failure" => PredicateResult.True(),
                    Exception ex when ex is InvalidOperationException => PredicateResult.True(),
#else
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: { } result } when result == "Failure" => PredicateResult.True(),
#endif
                    _ => PredicateResult.False(),
                },
            })
            .Build();

        #endregion
    }
}
