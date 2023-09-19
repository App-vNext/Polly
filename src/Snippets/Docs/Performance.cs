using System.Net.Http;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Performance
{
    public static async Task Lambda()
    {
        var resiliencePipeline = ResiliencePipeline.Empty;
        var userId = string.Empty;
        var cancellationToken = CancellationToken.None;

        #region perf-lambdas

        // This call allocates for each invocation since the "userId" variable is captured from the outer scope.
        await resiliencePipeline.ExecuteAsync(
            cancellationToken => GetMemberAsync(userId, cancellationToken),
            cancellationToken);

        // This approach uses a static lambda, avoiding allocations.
        // The "userId" is stored as state, and the lambda reads it.
        await resiliencePipeline.ExecuteAsync(
            static (state, cancellationToken) => GetMemberAsync(state, cancellationToken),
            userId,
            cancellationToken);

        #endregion
    }

    public static async Task SwitchExpressions()
    {
        #region perf-switch-expressions

        // Here, PredicateBuilder is used to configure which exceptions the retry strategy should handle.
        new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<SomeExceptionType>()
                    .Handle<InvalidOperationException>()
                    .Handle<HttpRequestException>()
            })
            .Build();

        // For optimal performance, it's recommended to use switch expressions over PredicateBuilder.
        new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = args => args.Outcome.Exception switch
                {
                    SomeExceptionType => PredicateResult.True(),
                    InvalidOperationException => PredicateResult.True(),
                    HttpRequestException => PredicateResult.True(),
                    _ => PredicateResult.False()
                }
            })
            .Build();

        #endregion
    }

    private static ValueTask GetMemberAsync(string id, CancellationToken token) => default;
}
