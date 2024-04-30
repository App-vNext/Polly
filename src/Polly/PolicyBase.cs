namespace Polly;

/// <summary>
/// Implements elements common to both non-generic and generic policies, and sync and async policies.
/// </summary>
public abstract partial class PolicyBase
{
    /// <summary>
    /// Defines a value to use for continueOnCaptureContext, when none is supplied.
    /// </summary>
    internal const bool DefaultContinueOnCapturedContext = false;

    /// <summary>
    /// Predicates indicating which exceptions the policy handles.
    /// </summary>
    protected internal ExceptionPredicates ExceptionPredicates { get; }

    /// <summary>
    /// Defines a CancellationToken to use, when none is supplied.
    /// </summary>
    internal readonly CancellationToken DefaultCancellationToken = CancellationToken.None;

    internal static ExceptionType GetExceptionType(ExceptionPredicates exceptionPredicates, Exception exception)
    {
        bool isExceptionTypeHandledByThisPolicy = exceptionPredicates.FirstMatchOrDefault(exception) != null;

        return isExceptionTypeHandledByThisPolicy
            ? ExceptionType.HandledByThisPolicy
            : ExceptionType.Unhandled;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyBase"/> class.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    private protected PolicyBase(ExceptionPredicates exceptionPredicates) =>
        ExceptionPredicates = exceptionPredicates ?? ExceptionPredicates.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyBase"/> class.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> indicating which exceptions the policy should handle.</param>
    protected PolicyBase(PolicyBuilder policyBuilder)
        : this(policyBuilder?.ExceptionPredicates)
    {
    }
}

/// <summary>
/// Implements elements common to sync and async generic policies.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public abstract class PolicyBase<TResult> : PolicyBase
{
    /// <summary>
    /// Predicates indicating which results the policy handles.
    /// </summary>
    protected internal ResultPredicates<TResult> ResultPredicates { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyBase{TResult}"/> class.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
    private protected PolicyBase(
        ExceptionPredicates exceptionPredicates,
        ResultPredicates<TResult> resultPredicates)
    : base(exceptionPredicates) =>
        ResultPredicates = resultPredicates ?? ResultPredicates<TResult>.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyBase{TResult}"/> class.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> indicating which exceptions the policy should handle.</param>
    protected PolicyBase(PolicyBuilder<TResult> policyBuilder)
        : this(policyBuilder?.ExceptionPredicates, policyBuilder?.ResultPredicates)
    {
    }
}
