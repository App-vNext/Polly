namespace Polly.Utilities.Wrappers;

internal sealed class ResiliencePipelineSyncPolicy<TResult> : Policy<TResult>
{
    private readonly ResiliencePipeline<TResult> _strategy;

    public ResiliencePipelineSyncPolicy(ResiliencePipeline<TResult> strategy) => _strategy = strategy;

    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
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
