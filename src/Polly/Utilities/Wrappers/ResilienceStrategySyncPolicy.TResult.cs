namespace Polly.Utilities.Wrappers;

internal sealed class ResilienceStrategySyncPolicy<TResult> : Policy<TResult>
{
    private readonly ResilienceStrategy<TResult> _strategy;

    public ResilienceStrategySyncPolicy(ResilienceStrategy<TResult> strategy) => _strategy = strategy;

    protected sealed override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, true);

        try
        {
            return _strategy.Execute(
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
