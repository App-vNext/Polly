using System;
using System.Threading.Tasks;

namespace Polly.Utils;

internal static class DefaultPredicates<TArgs, TResult>
{
    public static readonly Func<OutcomeArguments<TResult, TArgs>, ValueTask<bool>> HandleOutcome = args => args.Exception switch
    {
        OperationCanceledException => PredicateResult.False,
        Exception => PredicateResult.True,
        _ => PredicateResult.False
    };
}
