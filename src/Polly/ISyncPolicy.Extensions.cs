namespace Polly;

/// <summary>
/// Contains extensions methods on <see cref="ISyncPolicy"/>.
/// </summary>
public static class ISyncPolicyExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="ISyncPolicy"/> into a generic <see cref="ISyncPolicy{TResult}"/> for handling only executions returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>This method allows you to convert a non-generic <see cref="ISyncPolicy"/> into a generic <see cref="ISyncPolicy{TResult}"/> for contexts such as variables or parameters which may explicitly require a generic <see cref="ISyncPolicy{TResult}"/>. </remarks>
    /// <param name="policy">The non-generic <see cref="ISyncPolicy"/>  to convert to a generic <see cref="ISyncPolicy{TResult}"/>.</param>
    /// <returns>A generic <see cref="ISyncPolicy{TResult}"/> version of the supplied non-generic <see cref="ISyncPolicy"/>.</returns>
    public static ISyncPolicy<TResult> AsPolicy<TResult>(this ISyncPolicy policy) =>
        policy.Wrap(Policy.NoOp<TResult>());
}

