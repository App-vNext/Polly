namespace Polly.Utilities.Wrappers;

internal sealed class ResilienceStrategyAsyncPolicy<TResult> : AsyncPolicy<TResult>
{
    private readonly ResilienceStrategy<TResult> _strategy;

    public ResilienceStrategyAsyncPolicy(ResilienceStrategy<TResult> strategy) => _strategy = strategy;

    protected override async Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
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
