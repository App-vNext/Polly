namespace Polly.Specs.Helpers;

public static class ContextualPolicyTResultExtensionsAsync
{
    public static Task<TResult> RaiseResultSequenceAsync<TResult>(
        this AsyncPolicy<TResult> policy,
        IDictionary<string, object> contextData,
        params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceAsync(contextData, CancellationToken.None, [.. resultsToRaise]);

    public static Task<TResult> RaiseResultSequenceAsync<TResult>(this AsyncPolicy<TResult> policy, IDictionary<string, object> contextData, CancellationToken cancellationToken, IEnumerable<TResult> resultsToRaise)
    {
        var enumerator = resultsToRaise.GetEnumerator();

        return policy.ExecuteAsync((_, _) =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), "Not enough TResult values in resultsToRaise.");
            }

            return Task.FromResult(enumerator.Current);
        }, contextData, cancellationToken);
    }

    public static Task<PolicyResult<TResult>> RaiseResultSequenceOnExecuteAndCaptureAsync<TResult>(this AsyncPolicy<TResult> policy, IDictionary<string, object> contextData, params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceOnExecuteAndCaptureAsync(contextData, resultsToRaise.ToList());

    public static Task<PolicyResult<TResult>> RaiseResultSequenceOnExecuteAndCaptureAsync<TResult>(this AsyncPolicy<TResult> policy, IDictionary<string, object> contextData, IEnumerable<TResult> resultsToRaise)
    {
        var enumerator = resultsToRaise.GetEnumerator();

        return policy.ExecuteAndCaptureAsync(_ =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), "Not enough TResult values in resultsToRaise.");
            }

            return Task.FromResult(enumerator.Current);
        }, contextData);
    }
}
