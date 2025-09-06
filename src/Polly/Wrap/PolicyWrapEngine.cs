namespace Polly.Wrap;

internal static class PolicyWrapEngine
{
    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> func,
        Context context,
        ISyncPolicy<TResult> outerPolicy,
        ISyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> func,
        Context context,
        ISyncPolicy<TResult> outerPolicy,
        ISyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> func,
        Context context,
        ISyncPolicy outerPolicy,
        ISyncPolicy<TResult> innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> func,
        Context context,
        ISyncPolicy outerPolicy,
        ISyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(func, ctx, ct), context, cancellationToken);

    internal static void Implementation(
        Action<Context, CancellationToken> action,
        Context context,
        ISyncPolicy outerPolicy,
        ISyncPolicy innerPolicy,
        CancellationToken cancellationToken) =>
        outerPolicy.Execute((ctx, ct) => innerPolicy.Execute(action, ctx, ct), context, cancellationToken);
}
