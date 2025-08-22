using Scenario = Polly.Specs.Helpers.PolicyTResultExtensions.ResultAndOrCancellationScenario;

namespace Polly.Specs.Helpers;

public static class PolicyTResultExtensions
{
    public static TResult RaiseResultSequence<TResult>(this Policy<TResult> policy, params TResult[] resultsToRaise) =>
        policy.RaiseResultSequence(resultsToRaise.ToList());

    public static TResult RaiseResultSequence<TResult>(this Policy<TResult> policy, IEnumerable<TResult> resultsToRaise)
    {
        using var enumerator = resultsToRaise.GetEnumerator();
        return policy.Execute(() =>
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentOutOfRangeException(nameof(resultsToRaise), $"Not enough {typeof(TResult).Name}  values in {nameof(resultsToRaise)}.");
            }

            return enumerator.Current;
        });
    }

    public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy, params object[] resultsOrExceptionsToRaise) =>
        policy.RaiseResultAndOrExceptionSequence(resultsOrExceptionsToRaise.ToList());

    public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy, IEnumerable<object> resultsOrExceptionsToRaise)
    {
        using var enumerator = resultsOrExceptionsToRaise.GetEnumerator();
        return policy.Execute(() =>
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
                return result;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(resultsOrExceptionsToRaise), $"Value is not either an {nameof(Exception)} or {typeof(TResult).Name}.");
            }
        });
    }

    public class ResultAndOrCancellationScenario
    {
        public int? AttemptDuringWhichToCancel;

        public bool ActionObservesCancellation = true;
    }

    public static TResult RaiseResultSequenceAndOrCancellation<TResult>(
        this Policy<TResult> policy,
        Scenario scenario,
        CancellationTokenSource cancellationTokenSource,
        Action onExecute,
        params TResult[] resultsToRaise) =>
        policy.RaiseResultSequenceAndOrCancellation(
            scenario,
            cancellationTokenSource,
            onExecute,
            resultsToRaise.ToList());

    public static TResult RaiseResultSequenceAndOrCancellation<TResult>(
        this Policy<TResult> policy,
        Scenario scenario,
        CancellationTokenSource cancellationTokenSource,
        Action onExecute,
        IEnumerable<TResult> resultsToRaise)
    {
        int counter = 0;

        CancellationToken cancellationToken = cancellationTokenSource.Token;

        using var enumerator = resultsToRaise.GetEnumerator();
        return policy.Execute(ct =>
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

            return enumerator.Current;
        }, cancellationToken);
    }
}

