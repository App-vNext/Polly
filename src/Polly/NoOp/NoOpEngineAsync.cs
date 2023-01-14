namespace Polly.NoOp;

internal static partial class NoOpEngine
{
    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        action(context, cancellationToken);
}
