using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var enumerator = resultsToRaise.GetEnumerator();

            return policy.Execute(() =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsToRaise", "Not enough TResult values in resultsToRaise.");
                }

                return enumerator.Current;
            });
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy, params object[] resultsOrExceptionsToRaise)
        {
            return policy.RaiseResultAndOrExceptionSequence(resultsOrExceptionsToRaise.ToList());
        }

        public static TResult RaiseResultAndOrExceptionSequence<TResult>(this Policy<TResult> policy, IEnumerable<object> resultsOrExceptionsToRaise)
        {
            var enumerator = resultsOrExceptionsToRaise.GetEnumerator();

            return policy.Execute(() =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsOrExceptionsToRaise", "Not enough TResult values in resultsOrExceptionsToRaise.");
                }

                object current = enumerator.Current;
                if (current is Exception)
                {
                    throw (Exception) current;
                }
                else if (current is TResult)
                {
                    return (TResult)current;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("resultsOrExceptionsToRaise", "Value is not either an Exception or TResult.");
                }
            });
        }
    }
}
