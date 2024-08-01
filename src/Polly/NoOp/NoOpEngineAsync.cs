#nullable enable
namespace Polly.NoOp;

internal static partial class NoOpEngine
{
    [DebuggerDisableUserUnhandledExceptions]
    internal static Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        action(context, cancellationToken);
}
