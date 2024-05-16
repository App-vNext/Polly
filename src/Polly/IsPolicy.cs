namespace Polly;

/// <summary>
/// A marker interface identifying Polly policies of all types, and containing properties common to all policies.
/// </summary>
#pragma warning disable IDE1006
public interface IsPolicy
#pragma warning restore IDE1006
{
    /// <summary>
    /// Gets a key intended to be unique to each policy instance, which is passed with executions as the <see cref="Context.PolicyKey"/> property.
    /// </summary>
    string PolicyKey { get; }
}
