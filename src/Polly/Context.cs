namespace Polly;

/// <summary>
/// Context that carries with a single execution through a Policy.   Commonly-used properties are directly on the class.  Backed by a dictionary of string key / object value pairs, to which user-defined values may be added.
/// <remarks>Do not re-use an instance of <see cref="Context"/> across more than one call through .Execute(...) or .ExecuteAsync(...).</remarks>
/// </summary>
public partial class Context
{
    internal static Context None() => [];

    private Guid? _correlationId;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class, with the specified <paramref name="operationKey"/>.
    /// </summary>
    /// <param name="operationKey">The operation key.</param>
    public Context(string operationKey) =>
        OperationKey = operationKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
    {
    }

    /// <summary>
    /// When execution is through a <see cref="PolicyWrap"/>, identifies the PolicyWrap executing the current delegate by returning the <see cref="PolicyBase.PolicyKey"/> of the outermost layer in the PolicyWrap; otherwise, null.
    /// </summary>
    public string PolicyWrapKey { get; internal set; }

    /// <summary>
    /// The <see cref="PolicyBase.PolicyKey"/> of the policy instance executing the current delegate.
    /// </summary>
    public string PolicyKey { get; internal set; }

    /// <summary>
    /// A key unique to the call site of the current execution.
    /// <remarks>Policy instances are commonly reused across multiple call sites.  Set an OperationKey so that logging and metrics can distinguish usages of policy instances at different call sites.</remarks>
    /// <remarks>The value is set by using the <see cref="Context(string)"/> constructor taking an operationKey parameter.</remarks>
    /// </summary>
    public string OperationKey { get; }

    /// <summary>
    /// A Guid guaranteed to be unique to each execution.
    /// <remarks>Acts as a correlation id so that events specific to a single execution can be identified in logging and telemetry.</remarks>
    /// </summary>
    public Guid CorrelationId
    {
        get
        {
            _correlationId ??= Guid.NewGuid();

            return _correlationId.Value;
        }
    }
}
