namespace Polly.Utilities.Wrappers;

internal class ResilienceStrategySyncPolicy : Policy
{
    private readonly ResilienceStrategy strategy;

    public ResilienceStrategySyncPolicy(ResilienceStrategy strategy) => this.strategy = strategy;

    protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, true);

        try
        {
            strategy.Execute(
                static (context, state) =>
                {
                    state(context.GetContext(), context.CancellationToken);
                },
                resilienceContext,
                action);
        }
        finally
        {
            ResilienceContextFactory.Restore(resilienceContext);
        }
    }

    protected sealed override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, true);

        try
        {
            return strategy.Execute(
                static (context, state) =>
                {
                    return state(context.GetContext(), context.CancellationToken);
                },
                resilienceContext,
                action);
        }
        finally
        {
            ResilienceContextFactory.Restore(resilienceContext);
        }
    }
}
