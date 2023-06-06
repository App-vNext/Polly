namespace Polly.Utilities.Wrappers;

internal sealed class ResilienceStrategyAsyncPolicy<TResult> : AsyncPolicy<TResult>
{
    private readonly ResilienceStrategy<TResult> _strategy;

    public ResilienceStrategyAsyncPolicy(ResilienceStrategy<TResult> strategy) => _strategy = strategy;

    protected sealed override async Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, continueOnCapturedContext);

        try
        {
            return await _strategy.ExecuteAsync(
                static async (context, state) =>
                {
                    return await state(context.GetContext(), context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                },
                resilienceContext,
                action).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            ResilienceContextFactory.Restore(resilienceContext);
        }
    }
}
