namespace Polly.Utilities.Wrappers;

internal sealed class ResiliencePipelineSyncPolicy : Policy
{
    private readonly ResiliencePipeline _strategy;

    public ResiliencePipelineSyncPolicy(ResiliencePipeline strategy) => _strategy = strategy;

    protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(
            context,
            cancellationToken,
            true,
            out var oldProperties);

        try
        {
            _strategy.Execute(
                static (context, state) =>
                {
                    state.action(state.context, context.CancellationToken);
                },
                resilienceContext,
                (action, context));
        }
        finally
        {
            ResilienceContextFactory.Cleanup(resilienceContext, oldProperties);
        }
    }

    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(
            context,
            cancellationToken,
            true,
            out var oldProperties);

        try
        {
            return _strategy.Execute(
                static (context, state) =>
                {
                    return state.action(state.context, context.CancellationToken);
                },
                resilienceContext,
                (action, context));
        }
        finally
        {
            ResilienceContextFactory.Cleanup(resilienceContext, oldProperties);
        }
    }
}
