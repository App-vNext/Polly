namespace Polly.Utilities.Wrappers;

internal class ResilienceStrategySyncPolicy<TResult> : Policy<TResult>
{
    private readonly ResilienceStrategy<TResult> strategy;

    public ResilienceStrategySyncPolicy(ResilienceStrategy<TResult> strategy) => this.strategy = strategy;

    protected sealed override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
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
