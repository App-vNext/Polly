#nullable enable
namespace Polly;

public partial class Policy
{
    /// <summary>
    /// Builds a NoOp <see cref="AsyncPolicy{TResult}"/> that will execute without any custom behavior.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <returns>The policy instance.</returns>
    public static AsyncNoOpPolicy<TResult> NoOpAsync<TResult>() => new();
}
