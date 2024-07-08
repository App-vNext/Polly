namespace Polly.Wrap;

internal static class AsyncPolicyWrapEngine
{
    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy<TResult> outerPolicy,
        IAsyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                func,
                ctx,
                ct,
                continueOnCapturedContext),
            context,
            cancellationToken,
            continueOnCapturedContext);

    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy<TResult> outerPolicy,
        IAsyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync<TResult>(
                func,
                ctx,
                ct,
                continueOnCapturedContext),
            context,
            cancellationToken,
            continueOnCapturedContext);

    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy outerPolicy,
        IAsyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync<TResult>(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                func,
                ctx,
                ct,
                continueOnCapturedContext),
            context,
            cancellationToken,
            continueOnCapturedContext);

    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy outerPolicy,
        IAsyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync<TResult>(
            (ctx, ct) => innerPolicy.ExecuteAsync<TResult>(
                func,
                ctx,
                ct,
                continueOnCapturedContext),
            context,
            cancellationToken,
            continueOnCapturedContext);

    internal static Task ImplementationAsync(Func<Context, CancellationToken, Task> action,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy outerPolicy,
        IAsyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                action,
                ctx,
                ct,
                continueOnCapturedContext),
            context,
            cancellationToken,
            continueOnCapturedContext);
}
