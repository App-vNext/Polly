using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Scenario = Polly.Specs.Helpers.PolicyTResultExtensions.ResultAndOrCancellationScenario;

namespace Polly.Specs.Helpers
{
    public static class PolicyTResultExtensions
    {
        public static TResult RaiseResultSequence<TResult>(this Policy<TResult> policy, params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequence(resultsToRaise.ToList());
        }

        public static TResult RaiseResultSequence<TResult>(this Policy<TResult> policy, IEnumerable<TResult> resultsToRaise) 
        {
            using (var enumerator = resultsToRaise.GetEnumerator())
            {
                return policy.Execute(() =>
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsToRaise), $"Not enough {typeof(TResult).Name}  values in {nameof(resultsToRaise)}.");
                    }

                    return enumerator.Current;
                });
            }
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy, params object[] resultsOrExceptionsToRaise)
        {
            return policy.RaiseResultAndOrExceptionSequence(resultsOrExceptionsToRaise.ToList());
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy,
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

        public static TResult RaiseResultSequenceAndOrCancellation<TResult>(this Policy<TResult> policy,
            Scenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute,
            params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                resultsToRaise.ToList());
        }

        public static TResult RaiseResultSequenceAndOrCancellation<TResult>(this Policy<TResult> policy, Scenario scenario, CancellationTokenSource cancellationTokenSource, Action onExecute, IEnumerable<TResult> resultsToRaise)
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
    }
}

