#nullable enable
namespace Polly.NoOp;

/// <summary>
/// A no op policy that can be applied to delegates.
/// </summary>
public class NoOpPolicy : Policy, INoOpPolicy
{
    internal NoOpPolicy()
    {
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return NoOpEngine.Implementation(action, context, cancellationToken);
    }
}

/// <summary>
/// A no op policy that can be applied to delegates returning a value of type <typeparamref name="TResult" />.
/// </summary>
/// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
public class NoOpPolicy<TResult> : Policy<TResult>, INoOpPolicy<TResult>
{
    internal NoOpPolicy()
    {
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return NoOpEngine.Implementation(action, context, cancellationToken);
    }
}
