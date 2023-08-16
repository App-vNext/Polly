namespace Polly;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the <see cref="ResilienceContextPool"/> when creating <see cref="ResilienceContext"/>.
/// </summary>
public readonly struct ResilienceContextCreationArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceContextCreationArguments"/> struct.
    /// </summary>
    /// <param name="operationKey">The operation key, if any.</param>
    /// <param name="continueOnCapturedContext">Value indicating whether to continue on captured context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public ResilienceContextCreationArguments(string? operationKey, bool? continueOnCapturedContext, CancellationToken cancellationToken)
    {
        OperationKey = operationKey;
        ContinueOnCapturedContext = continueOnCapturedContext;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the operation key, if any.
    /// </summary>
    public string? OperationKey { get; }

    /// <summary>
    /// Gets the value indicating whether to continue on captured context, if any.
    /// </summary>
    public bool? ContinueOnCapturedContext { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
