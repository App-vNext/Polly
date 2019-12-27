using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    public static class ContextualPolicyTResultExtensionsAsync
    {

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this IAsyncPolicy<TResult> policy,
    IDictionary<string, object> contextData,
    params TResult[] resultsToRaise)
        {
            return policy.RaiseResultSequenceAsync(contextData, CancellationToken.None, resultsToRaise.ToList());
        }

        public static Task<TResult> RaiseResultSequenceAsync<TResult>(this IAsyncPolicy<TResult> policy,
            IDictionary<string, object> contextData, CancellationToken cancellationToken,
            IEnumerable<TResult> resultsToRaise)
        {
            using (var enumerator = resultsToRaise.GetEnumerator())
            {
                return policy.ExecuteAsync((ctx, ct) =>
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(resultsToRaise),
                            "Not enough TResult values in resultsToRaise.");
                    }

                    return Task.FromResult(enumerator.Current);
                }, contextData, cancellationToken);
            }
        }
    }
}
