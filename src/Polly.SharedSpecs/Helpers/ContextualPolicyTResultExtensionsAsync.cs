using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyTResultExtensionsAsync
    {

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this Policy<TResult> policy,
    IDictionary<string, object> contextData,
    params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceAsync(contextData, CancellationToken.None, resultsToRaise.ToList());
        }

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this Policy<TResult> policy, IDictionary<string, object> contextData, CancellationToken cancellationToken, IEnumerable<TResult> resultsToRaise)
        {
            var enumerator = resultsToRaise.GetEnumerator();

            return policy.ExecuteAsync(ct =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsToRaise", "Not enough TResult values in resultsToRaise.");
                }

                return Task.FromResult(enumerator.Current);
            }, contextData, cancellationToken);
        }

        public static Task<PolicyResult<TResult>> RaiseResultSequenceOnExecuteAndCaptureAsync<TResult>(this Policy<TResult> policy, IDictionary<string, object> contextData, params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceOnExecuteAndCaptureAsync(contextData, resultsToRaise.ToList());
        }

        public static Task<PolicyResult<TResult>> RaiseResultSequenceOnExecuteAndCaptureAsync<TResult>(this Policy<TResult> policy, IDictionary<string, object> contextData, IEnumerable<TResult> resultsToRaise)
        {
            var enumerator = resultsToRaise.GetEnumerator();

            return policy.ExecuteAndCaptureAsync(() =>
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("resultsToRaise", "Not enough TResult values in resultsToRaise.");
                }

                return Task.FromResult(enumerator.Current);
            }, contextData);
        }
    }
}
