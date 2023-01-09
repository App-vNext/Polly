using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Resilience;
using Resilience.Polly;

namespace Polly.Strategies;

internal class StrategySyncPolicy : Policy, ISyncPolicy
{
    private readonly IResilienceStrategy _strategy;

    public StrategySyncPolicy(IResilienceStrategy strategy) => _strategy = strategy;

    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContext.Get(cancellationToken);
        resilienceContext.ContinueOnCapturedContext = true;
        resilienceContext.IsSynchronous = true;
        resilienceContext.IsVoid = false;
        resilienceContext.ResultType = typeof(TResult);

        resilienceContext.Update(context);

        return _strategy.ExecuteAsync(
            (context, state) =>
            {
                var result = state.action(state.context, context.CancellationToken);
                return new ValueTask<TResult>(result);
            },
            resilienceContext,
            (action, context)).GetAwaiter().GetResult();
    }

}
