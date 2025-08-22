namespace Polly.Specs.Helpers;

public static class ContextualPolicyTResultExtensions
{
    public static TResult RaiseResultSequence<TResult>(
        this Policy<TResult> policy,
        IDictionary<string, object> contextData,
        params TResult[] resultsToRaise) =>
        policy.RaiseResultSequence(contextData, resultsToRaise.ToList());

    public static TResult RaiseResultSequence<TResult>(
        this Policy<TResult> policy,
        IDictionary<string, object> contextData,
        IEnumerable<TResult> resultsToRaise)
    {
        var enumerator = resultsToRaise.GetEnumerator();

        return policy.Execute(_ =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), "Not enough TResult values in resultsToRaise.");
            }

            return enumerator.Current;
        }, contextData);
    }

    public static PolicyResult<TResult> RaiseResultSequenceOnExecuteAndCapture<TResult>(
        this Policy<TResult> policy,
        IDictionary<string, object> contextData,
        params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceOnExecuteAndCapture(contextData, resultsToRaise.ToList());

    public static PolicyResult<TResult> RaiseResultSequenceOnExecuteAndCapture<TResult>(
        this Policy<TResult> policy,
        IDictionary<string, object> contextData,
        IEnumerable<TResult> resultsToRaise)
    {
        var enumerator = resultsToRaise.GetEnumerator();

        return policy.ExecuteAndCapture(_ =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), "Not enough TResult values in resultsToRaise.");
            }

            return enumerator.Current;
        }, contextData);
    }
}
