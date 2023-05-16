namespace Polly.Strategy;

/// <summary>
/// The recommended shape for arguments used by individual strategies.
/// </summary>
public interface IResilienceArguments
{
    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    ResilienceContext Context { get; }
}
