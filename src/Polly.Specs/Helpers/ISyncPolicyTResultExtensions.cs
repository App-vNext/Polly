using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Scenario = Polly.Specs.Helpers.ISyncPolicyTResultExtensions.ResultAndOrCancellationScenario;

namespace Polly.Specs.Helpers
{
    public static class ISyncPolicyTResultExtensions
    {
        public static TResult RaiseResultSequence<TResult>(this ISyncPolicy<TResult> policy, params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequence(resultsToRaise.ToList());
        }

        public static TResult RaiseResultSequence<TResult>(this ISyncPolicy<TResult> policy, IEnumerable<TResult> resultsToRaise)
        {
            return policy.RaiseResultSequence(new Dictionary<string, object>(0), resultsToRaise);
        }

        public static TResult RaiseResultSequence<TResult>(this ISyncPolicy<TResult> policy,
            IDictionary<string, object> contextData,
            params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequence(contextData, resultsToRaise.ToList());
        }

        public static TResult RaiseResultSequence<TResult>(this ISyncPolicy<TResult> policy,
            IDictionary<string, object> contextData,
            IEnumerable<TResult> resultsToRaise)
        {
            using (var enumerator = resultsToRaise.GetEnumerator())
            {
                return policy.Execute(ctx =>
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsToRaise), $"Not enough {typeof(TResult).Name}  values in {nameof(resultsToRaise)}.");
                    }

                    return enumerator.Current;
                }, contextData);
            }
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this ISyncPolicy<TResult> policy, params object[] resultsOrExceptionsToRaise)
        {
            return policy.RaiseResultAndOrExceptionSequence(resultsOrExceptionsToRaise.ToList());
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this ISyncPolicy<TResult> policy,
            IEnumerable<object> resultsOrExceptionsToRaise)
        {
            using (var enumerator = resultsOrExceptionsToRaise.GetEnumerator())
            {

                return policy.Execute(() =>
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsOrExceptionsToRaise), $"Not enough {typeof(TResult).Name} values in {nameof(resultsOrExceptionsToRaise)}.");
                    }

                    object current = enumerator.Current;
                    if (current is Exception)
                    {
                        throw (Exception) current;
                    }
                    else if (current is TResult)
                    {
                        return (TResult) current;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsOrExceptionsToRaise), $"Value is not either an {typeof(Exception).Name} or {typeof(TResult).Name}.");
                    }
                });
            }
        }

        public class ResultAndOrCancellationScenario
        {
            public int? AttemptDuringWhichToCancel = null;

            public bool ActionObservesCancellation = true;
        }

        public static TResult RaiseResultSequenceAndOrCancellation<TResult>(this ISyncPolicy<TResult> policy,
            Scenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute,
            params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                resultsToRaise.ToList());
        }

        public static TResult RaiseResultSequenceAndOrCancellation<TResult>(this ISyncPolicy<TResult> policy, Scenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, IEnumerable<TResult> resultsToRaise)
        {
            int counter = 0;

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            using (var enumerator = resultsToRaise.GetEnumerator())
            {
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

        public static PolicyResult<TResult> RaiseResultSequenceOnExecuteAndCapture<TResult>(this ISyncPolicy<TResult> policy,
            IDictionary<string, object> contextData,
            params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceOnExecuteAndCapture(contextData, resultsToRaise.ToList());
        }

        public static PolicyResult<TResult> RaiseResultSequenceOnExecuteAndCapture<TResult>(this ISyncPolicy<TResult> policy,
            IDictionary<string, object> contextData,
            IEnumerable<TResult> resultsToRaise)
        {
            using (var enumerator = resultsToRaise.GetEnumerator())
            {
                return policy.ExecuteAndCapture(ctx =>
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsToRaise),
                            "Not enough TResult values in resultsToRaise.");
                    }

                    return enumerator.Current;
                }, contextData);
            }
        }
    }
}

