namespace Snippets.Docs;

internal static class General
{
    public static async Task SynchronizationContext()
    {
        ResiliencePipeline pipeline = ResiliencePipeline.Empty;

        #region synchronization-context

        // Retrieve an instance of ResilienceContext from the pool
        // with the ContinueOnCapturedContext property set to true
        ResilienceContext context = ResilienceContextPool.Shared.Get(continueOnCapturedContext: true);

        await pipeline.ExecuteAsync(
            static async context =>
            {
                // Execute your code, honoring the ContinueOnCapturedContext setting
                await MyMethodAsync(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
            },
            context);

        // Optionally, return the ResilienceContext instance back to the pool
        // to minimize allocations and enhance performance
        ResilienceContextPool.Shared.Return(context);

        #endregion

        static async Task MyMethodAsync(CancellationToken cancellationToken) => await Task.Delay(100, cancellationToken);
    }

    public static async Task CancellationTokenSample()
    {
        ResiliencePipeline pipeline = ResiliencePipeline.Empty;
        var cancellationToken = CancellationToken.None;

        #region cancellation-token

        // Execute your code with cancellation support
        await pipeline.ExecuteAsync(
            static async token => await MyMethodAsync(token),
            cancellationToken);

        // Use ResilienceContext for more advanced scenarios
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken: cancellationToken);

        await pipeline.ExecuteAsync(
            async context => await MyMethodAsync(context.CancellationToken),
            context);

        #endregion

        static async Task MyMethodAsync(CancellationToken cancellationToken) => await Task.Delay(100, cancellationToken);
    }
}
