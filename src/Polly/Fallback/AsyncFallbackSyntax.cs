#nullable enable

namespace Polly;

/// <summary>
/// Fluent API for defining a Fallback <see cref="AsyncPolicy"/>.
/// </summary>
public static class AsyncFallbackSyntax
{
    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, asynchronously calls <paramref name="fallbackAction"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<CancellationToken, Task> fallbackAction)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));

        Func<Exception, Task> doNothing = _ => TaskHelper.EmptyTask;
        return policyBuilder.FallbackAsync(
            fallbackAction,
            doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception; then asynchronously calls <paramref name="fallbackAction"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<CancellationToken, Task> fallbackAction, Func<Exception, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync(
            (_, _, ct) => fallbackAction(ct),
            (outcome, _) => onFallbackAsync(outcome));
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<Context, CancellationToken, Task> fallbackAction, Func<Exception, Context, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync((_, ctx, ct) => fallbackAction(ctx, ct), onFallbackAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<Exception, Context, CancellationToken, Task> fallbackAction, Func<Exception, Context, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return new AsyncFallbackPolicy(policyBuilder, onFallbackAsync, fallbackAction);
    }
}

/// <summary>
/// Fluent API for defining an async Fallback policy governing executions returning TResult.
/// </summary>
public static class AsyncFallbackTResultSyntax
{
    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, returns <paramref name="fallbackValue"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue)
    {
        Func<DelegateResult<TResult>, Task> doNothing = _ => TaskHelper.EmptyTask;
        return policyBuilder.FallbackAsync(
            _ => Task.FromResult(fallbackValue),
            doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, asynchronously calls <paramref name="fallbackAction"/> and returns its result.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, Task<TResult>> fallbackAction)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));

        Func<DelegateResult<TResult>, Task> doNothing = _ => TaskHelper.EmptyTask;
        return policyBuilder.FallbackAsync(
            fallbackAction,
            doNothing);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result; then returns <paramref name="fallbackValue"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Func<DelegateResult<TResult>, Task> onFallbackAsync)
    {
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync(
            (_, _, _) => Task.FromResult(fallbackValue),
            (outcome, _) => onFallbackAsync(outcome));
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync(
            (_, _, ct) => fallbackAction(ct),
            (outcome, _) => onFallbackAsync(outcome));
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then returns <paramref name="fallbackValue"/>.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
    {
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync(
            (_, _, _) => Task.FromResult(fallbackValue),
            onFallbackAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return policyBuilder.FallbackAsync((_, ctx, ct) => fallbackAction(ctx, ct), onFallbackAsync);
    }

    /// <summary>
    /// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.
    /// </summary>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="fallbackAction">The fallback delegate.</param>
    /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
    /// <exception cref="ArgumentNullException">fallbackAction</exception>
    /// <exception cref="ArgumentNullException">onFallbackAsync</exception>
    /// <returns>The policy instance.</returns>
    public static AsyncFallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
    {
        if (fallbackAction == null)
            throw new ArgumentNullException(nameof(fallbackAction));
        if (onFallbackAsync == null)
            throw new ArgumentNullException(nameof(onFallbackAsync));

        return new AsyncFallbackPolicy<TResult>(
                policyBuilder,
                onFallbackAsync,
                fallbackAction);
    }
}
