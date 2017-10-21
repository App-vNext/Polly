using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Fallback;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public static class FallbackSyntaxAsync
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, asynchronously calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<CancellationToken, Task> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            
            Func<Exception, Task> doNothing = _ => TaskHelper.EmptyTask;
            return policyBuilder.FallbackAsync(
                fallbackAction,
                doNothing
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception; then asynchronously calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<CancellationToken, Task> fallbackAction, Func<Exception, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync(
                (outcome, ctx, ct) => fallbackAction(ct),
                (outcome, context) => onFallbackAsync(outcome)
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallbackAsync take a Context input parameter.")]
        public static FallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<CancellationToken, Task> fallbackAction, Func<Exception, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync((outcome, ctx, ct) => fallbackAction(ct), onFallbackAsync);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<Context, CancellationToken, Task> fallbackAction, Func<Exception, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallbackAsync);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy FallbackAsync(this PolicyBuilder policyBuilder, Func<Exception, Context, CancellationToken, Task> fallbackAction, Func<Exception, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return new FallbackPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => FallbackEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    (outcome, ctx) => onFallbackAsync(outcome.Exception, ctx),
                    async (outcome, ctx, ct) => { await fallbackAction(outcome.Exception, ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    cancellationToken,
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates);
        }
    }

    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public static class FallbackTResultSyntaxAsync
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, returns <paramref name="fallbackValue"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue)
        {
            Func<DelegateResult<TResult>, Task> doNothing = _ => TaskHelper.EmptyTask;
            return policyBuilder.FallbackAsync(
                ct => TaskHelper.FromResult(fallbackValue),
                doNothing
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, Task<TResult>> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            Func<DelegateResult<TResult>, Task> doNothing = _ => TaskHelper.EmptyTask;
            return policyBuilder.FallbackAsync(
                fallbackAction,
                doNothing
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result; then returns <paramref name="fallbackValue"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Func<DelegateResult<TResult>, Task> onFallbackAsync)
        {
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync(
                (outcome, ctx, ct) => TaskHelper.FromResult(fallbackValue),
                (outcome, context) => onFallbackAsync(outcome)
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync(
                (outcome, ctx, ct) => fallbackAction(ct),
                (outcome, context) => onFallbackAsync(outcome)
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then returns <paramref name="fallbackValue"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
        {
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync(
                (outcome, ctx, ct) => TaskHelper.FromResult(fallbackValue),
                onFallbackAsync
                );
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallbackAsync take a Context input parameter.")]
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync((outcome, ctx, ct) => fallbackAction(ct), onFallbackAsync);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return policyBuilder.FallbackAsync((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallbackAsync);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback delegate.</param>
        /// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallbackAsync</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> FallbackAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Context, Task> onFallbackAsync)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallbackAsync == null) throw new ArgumentNullException(nameof(onFallbackAsync));

            return new FallbackPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => FallbackEngine.ImplementationAsync(
                    action,
                    context,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    onFallbackAsync,
                    fallbackAction,
                    cancellationToken,
                    continueOnCapturedContext),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }
    }
}