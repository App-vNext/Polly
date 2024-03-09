namespace Polly;

/// <summary>
/// A marker interface identifying Polly policies of all types, and containing properties common to all policies.
/// </summary>
public interface IsPolicy
{
    /// <summary>
    /// A key intended to be unique to each policy instance, which is passed with executions as the <see cref="Context.PolicyKey"/> property.
    /// </summary>
    string PolicyKey { get; }
}
