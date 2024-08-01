#nullable enable
namespace Polly.NoOp;

internal static partial class NoOpEngine
{
    [DebuggerDisableUserUnhandledExceptions]
    internal static TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        action(context, cancellationToken);
}
