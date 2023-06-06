namespace Polly.Utilities.Wrappers;

internal sealed class ResilienceStrategySyncPolicy : Policy
{
    private readonly ResilienceStrategy _strategy;

    public ResilienceStrategySyncPolicy(ResilienceStrategy strategy) => _strategy = strategy;

    protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, true);

        try
        {
            _strategy.Execute(
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
