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
    }
}
