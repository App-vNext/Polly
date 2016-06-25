using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class PolicyTResultExtensionsAsync
    {
        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this Policy<TResult> policy, params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceAsync(resultsToRaise.ToList());
        }

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this Policy<TResult> policy, IEnumerable<TResult> resultsToRaise)
        {
            return policy.RaiseResultSequenceAsync(default(CancellationToken), resultsToRaise);
        }

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this Policy<TResult> policy, CancellationToken cancellationToken, IEnumerable<TResult> resultsToRaise)
        {
            var enumerator = resultsToRaise.GetEnumerator();

            return policy.ExecuteAsync(ct =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsToRaise", "Not enough TResult values in resultsToRaise.");
                }

                return Task.FromResult(enumerator.Current);
            }, cancellationToken);
        }

        public static Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(this Policy<TResult> policy, params object[] resultsOrExceptionsToRaise)
        {
            return policy.RaiseResultAndOrExceptionSequenceAsync(resultsOrExceptionsToRaise.ToList());
        }

        public static Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(this Policy<TResult> policy,
            IEnumerable<object> resultsOrExceptionsToRaise)
        {
            return policy.RaiseResultAndOrExceptionSequenceAsync(CancellationToken.None, resultsOrExceptionsToRaise);
        }

        public static Task<TResult> RaiseResultAndOrExceptionSequenceAsync<TResult>(this Policy<TResult> policy, CancellationToken cancellationToken, IEnumerable<object> resultsOrExceptionsToRaise)
        {
            var enumerator = resultsOrExceptionsToRaise.GetEnumerator();

            return policy.ExecuteAsync(ct =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsOrExceptionsToRaise", "Not enough TResult values in resultsOrExceptionsToRaise.");
                }

                object current = enumerator.Current;
                if (current is Exception)
                {
                    throw (Exception)current;
                }
                else if (current is TResult)
                {
                    return Task.FromResult((TResult)current);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("resultsOrExceptionsToRaise", "Value is not either an Exception or TResult.");
                }
            }, cancellationToken);
        }
    }
}
