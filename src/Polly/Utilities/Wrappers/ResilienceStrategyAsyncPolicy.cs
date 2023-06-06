namespace Polly.Utilities.Wrappers;

internal sealed class ResilienceStrategyAsyncPolicy : AsyncPolicy
{
    private readonly ResilienceStrategy _strategy;

    public ResilienceStrategyAsyncPolicy(ResilienceStrategy strategy) => _strategy = strategy;

    protected sealed override async Task ImplementationAsync(
        Func<Context, CancellationToken, Task> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        var resilienceContext = ResilienceContextFactory.Create(context, cancellationToken, continueOnCapturedContext);

        try
        {
            await _strategy.ExecuteAsync(
                static async (context, state) =>
                {
                    await state(context.GetContext(), context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                },
                resilienceContext,
                action).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            ResilienceContextFactory.Restore(resilienceContext);
        }
    }

    protected override async Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
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
