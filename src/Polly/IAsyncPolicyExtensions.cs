namespace Polly;

/// <summary>
/// Contains extensions methods on <see cref="IAsyncPolicy"/>.
/// </summary>
public static class IAsyncPolicyExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="IAsyncPolicy"/> into a generic <see cref="IAsyncPolicy{TResult}"/> for handling only executions returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>This method allows you to convert a non-generic <see cref="IAsyncPolicy"/> into a generic <see cref="IAsyncPolicy{TResult}"/> for contexts such as variables or parameters which may explicitly require a generic <see cref="IAsyncPolicy{TResult}"/>. </remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="policy">The non-generic <see cref="IAsyncPolicy"/>  to convert to a generic <see cref="IAsyncPolicy{TResult}"/>.</param>
    /// <returns>A generic <see cref="IAsyncPolicy{TResult}"/> version of the supplied non-generic <see cref="IAsyncPolicy"/>.</returns>
    public static IAsyncPolicy<TResult> AsAsyncPolicy<TResult>(this IAsyncPolicy policy) =>
        policy.WrapAsync(Policy.NoOpAsync<TResult>());
}

