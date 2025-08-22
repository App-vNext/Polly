using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.Helpers;

public static class PolicyTResultExtensionsAsync
{
    public static Task<TResult> RaiseResultSequenceAsync<TResult>(this AsyncPolicy<TResult> policy, params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceAsync(resultsToRaise.ToList());

    public static Task<TResult> RaiseResultSequenceAsync<TResult>(this AsyncPolicy<TResult> policy, IEnumerable<TResult> resultsToRaise) =>
        policy.RaiseResultSequenceAsync(default, resultsToRaise);

    public static async Task<TResult> RaiseResultSequenceAsync<TResult>(
        this AsyncPolicy<TResult> policy,
        CancellationToken cancellationToken,
        IEnumerable<TResult> resultsToRaise)
    {
        using var enumerator = resultsToRaise.GetEnumerator();
        return await policy.ExecuteAsync(_ =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), $"Not enough {typeof(TResult).Name} values in {nameof(resultsToRaise)}.");
            }

            return Task.FromResult(enumerator.Current);
        }, cancellationToken);
    }

    public static Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(this AsyncPolicy<TResult> policy, params object[] resultsOrExceptionsToRaise) =>
        policy.RaiseResultAndOrExceptionSequenceAsync(resultsOrExceptionsToRaise.ToList());

    public static Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(this AsyncPolicy<TResult> policy, IEnumerable<object> resultsOrExceptionsToRaise) =>
        policy.RaiseResultAndOrExceptionSequenceAsync(CancellationToken.None, resultsOrExceptionsToRaise);

    public static async Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(
        this AsyncPolicy<TResult> policy,
        CancellationToken cancellationToken,
        IEnumerable<object> resultsOrExceptionsToRaise)
    {
        using var enumerator = resultsOrExceptionsToRaise.GetEnumerator();
        return await policy.ExecuteAsync(_ =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsOrExceptionsToRaise), $"Not enough {typeof(TResult).Name} values in {nameof(resultsOrExceptionsToRaise)}.");
            }

            object current = enumerator.Current;
            if (current is Exception exception)
            {
                throw exception;
            }
            else if (current is TResult result)
            {
                return Task.FromResult(result);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(resultsOrExceptionsToRaise),
                    $"Value is not either an {nameof(Exception)} or {typeof(TResult).Name}.");
            }
        }, cancellationToken);
    }

    public class ResultAndOrCancellationScenario
    {
        public int? AttemptDuringWhichToCancel;

        public bool ActionObservesCancellation = true;
    }

    public static Task<TResult> RaiseResultSequenceAndOrCancellationAsync<TResult>(
        this AsyncPolicy<TResult> policy,
        Scenario scenario,
        CancellationTokenSource cancellationTokenSource,
        Action onExecute,
        params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
            resultsToRaise.ToList());

    public static async Task<TResult> RaiseResultSequenceAndOrCancellationAsync<TResult>(
        this AsyncPolicy<TResult> policy,
        Scenario scenario,
        CancellationTokenSource cancellationTokenSource,
        Action onExecute,
        IEnumerable<TResult> resultsToRaise)
    {
        int counter = 0;

        CancellationToken cancellationToken = cancellationTokenSource.Token;

        using var enumerator = resultsToRaise.GetEnumerator();
        return await policy.ExecuteAsync(ct =>
        {
            onExecute();

            counter++;

            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), $"Not enough {typeof(TResult).Name}  values in {nameof(resultsToRaise)}.");
            }

            if (scenario.AttemptDuringWhichToCancel.HasValue && counter >= scenario.AttemptDuringWhichToCancel.Value)
            {
                cancellationTokenSource.Cancel();
            }

            if (scenario.ActionObservesCancellation)
            {
                ct.ThrowIfCancellationRequested();
            }

            return Task.FromResult(enumerator.Current);
        }, cancellationToken);
    }
}
