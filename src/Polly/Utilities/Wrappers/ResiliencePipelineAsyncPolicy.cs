namespace Polly.Utilities.Wrappers;

internal sealed class ResiliencePipelineAsyncPolicy : AsyncPolicy
{
    private readonly ResiliencePipeline _strategy;

    public ResiliencePipelineAsyncPolicy(ResiliencePipeline strategy) => _strategy = strategy;

    protected override async Task ImplementationAsync(
        Func<Context, CancellationToken, Task> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        var resilienceContext = ResilienceContextFactory.Create(
            context,
            cancellationToken,
            continueOnCapturedContext,
            out var oldProperties);

        try
        {
            await _strategy.ExecuteAsync(
                static async (resilienceContext, state) =>
                {
                    await state.action(state.context, resilienceContext.CancellationToken).ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
                },
                resilienceContext,
                (action, context)).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            ResilienceContextFactory.Cleanup(resilienceContext, oldProperties);
        }
    }

    protected override async Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        var resilienceContext = ResilienceContextFactory.Create(
            context,
            cancellationToken,
            continueOnCapturedContext,
            out var oldProperties);

        try
        {
            return await _strategy.ExecuteAsync(
                static async (resilienceContext, state) =>
                {
                    return await state.action(state.context, resilienceContext.CancellationToken).ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
                },
                resilienceContext,
                (action, context)).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            ResilienceContextFactory.Cleanup(resilienceContext, oldProperties);
        }
    }
}
