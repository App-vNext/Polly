namespace Polly;

/// <summary>
/// A resilience strategy that executes an user-provided callback without any additional logic.
/// </summary>
public sealed class NullResilienceStrategy : ResilienceStrategy
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResilienceStrategy"/>.
    /// </summary>
    public static readonly NullResilienceStrategy Instance = new();

    private NullResilienceStrategy()
        : base(NullResilienceStrategy<object>.Instance)
    {
    }
}
