namespace Polly.Utilities.Wrappers;

internal sealed class ResiliencePipelineSyncPolicy<TResult> : Policy<TResult>
{
    private readonly ResiliencePipeline<TResult> _pipeline;

    public ResiliencePipelineSyncPolicy(ResiliencePipeline<TResult> strategy) => _pipeline = strategy;

    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextFactory.Create(
            context,
            true,
            out var oldProperties,
            cancellationToken);

        try
        {
            return _pipeline.Execute(
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
