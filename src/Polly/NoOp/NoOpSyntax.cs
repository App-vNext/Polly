#nullable enable
namespace Polly;

public partial class Policy
{
    /// <summary>
    /// Builds a NoOp <see cref="Policy"/> that will execute without any custom behavior.
    /// </summary>
    /// <returns>The policy instance.</returns>
    public static NoOpPolicy NoOp() => new();
}
