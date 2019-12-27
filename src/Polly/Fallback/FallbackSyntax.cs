using System;
using System.Threading;
using Polly.Fallback;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Fallback policy. 
    /// </summary>
    public static class FallbackSyntax
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback(fallbackAction, onFallback: null);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<CancellationToken> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback(fallbackAction, onFallback: null);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action fallbackAction, Action<Exception> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), onFallback == null ? (Action<Exception, Context>)null : (exception, ctx) => onFallback(exception));
        }


        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<CancellationToken> fallbackAction, Action<Exception> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), onFallback == null ? (Action<Exception, Context>)null : (exception, ctx) => onFallback(exception));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Context> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx), onFallback);
        }
        
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Context, CancellationToken> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Exception, Context, CancellationToken> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return new FallbackPolicy(
                    policyBuilder,
                    onFallback,
                    fallbackAction);
        }
    }

    /// <summary>
    /// Fluent API for defining a Fallback policy governing executions returning TResult. 
    /// </summary>
    public static class FallbackTResultSyntax
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, returns <paramref name="fallbackValue"/> instead.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue)
        {
            return policyBuilder.Fallback(() => fallbackValue, onFallback: null);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback(fallbackAction, onFallback: null);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, TResult> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback(fallbackAction, onFallback: null);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then returns <paramref name="fallbackValue"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Action<DelegateResult<TResult>> onFallback)
        {
            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackValue, onFallback == null ? (Action<DelegateResult<TResult>, Context>)null : (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult> fallbackAction, Action<DelegateResult<TResult>> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), onFallback == null ? (Action<DelegateResult<TResult>, Context>)null : (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), onFallback == null ? (Action<DelegateResult<TResult>, Context>)null : (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then returns <paramref name="fallbackValue"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Action<DelegateResult<TResult>, Context> onFallback)
        {
            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackValue, onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static ISyncFallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            return new FallbackPolicy<TResult>(
                policyBuilder,
                onFallback,
                fallbackAction);
        }
    }
}
