namespace Polly.Wrap;

internal static class AsyncPolicyWrapEngine
{
    internal static Task<TResult> ImplementationAsync<TResult>(
       Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy<TResult> outerPolicy,
        IAsyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                func,
                ctx,
                continueOnCapturedContext,
                ct),
            context,
            continueOnCapturedContext,
            cancellationToken);

    internal static Task<TResult> ImplementationAsync<TResult>(
       Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy<TResult> outerPolicy,
        IAsyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync<TResult>(
                func,
                ctx,
                continueOnCapturedContext,
                ct),
            context,
            continueOnCapturedContext,
            cancellationToken);

    internal static Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> func,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy outerPolicy,
        IAsyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync<TResult>(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                func,
                ctx,
                continueOnCapturedContext,
                ct),
            context,
            continueOnCapturedContext,
            cancellationToken);

    internal static Task<TResult> ImplementationAsync<TResult>(
       Func<Context, CancellationToken, Task<TResult>> func,
       Context context,
       bool continueOnCapturedContext,
       IAsyncPolicy outerPolicy,
       IAsyncPolicy innerPolicy,
       CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync<TResult>(
            (ctx, ct) => innerPolicy.ExecuteAsync<TResult>(
                func,
                ctx,
                continueOnCapturedContext,
                ct),
            context,
            continueOnCapturedContext,
            cancellationToken);

    internal static Task ImplementationAsync(
        Func<Context, CancellationToken, Task> action,
        Context context,
        bool continueOnCapturedContext,
        IAsyncPolicy outerPolicy,
        IAsyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.ExecuteAsync(
            (ctx, ct) => innerPolicy.ExecuteAsync(
                action,
                ctx,
                continueOnCapturedContext,
                ct),
            context,
            continueOnCapturedContext,
            cancellationToken);
}
