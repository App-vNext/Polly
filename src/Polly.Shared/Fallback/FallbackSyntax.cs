using System;
using System.Threading;
using Polly.Fallback;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public static class FallbackSyntax
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            Action<Exception> doNothing = _ => { };
            return policyBuilder.Fallback(fallbackAction, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<CancellationToken> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            Action<Exception> doNothing = _ => { };
            return policyBuilder.Fallback(fallbackAction, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action fallbackAction, Action<Exception> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), (exception, ctx) => onFallback(exception));
        }


        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<CancellationToken> fallbackAction, Action<Exception> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), (exception, ctx) => onFallback(exception));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallback take a Context input parameter.")]
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Context> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallback take a Context input parameter.")]
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<CancellationToken> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Context, CancellationToken> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy Fallback(this PolicyBuilder policyBuilder, Action<Exception, Context, CancellationToken> fallbackAction, Action<Exception, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return new FallbackPolicy(
                (action, context, cancellationToken) => FallbackEngine.Implementation(
                    (ctx, ct) => { action(ctx, ct); return EmptyStruct.Instance; },
                    context,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates,
                    (outcome, ctx) => onFallback(outcome.Exception, ctx),
                    (outcome, ctx, ct) => { fallbackAction(outcome.Exception, ctx, ct); return EmptyStruct.Instance; }),
                policyBuilder.ExceptionPredicates);
        }
    }

    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public static class FallbackTResultSyntax
    {
        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, returns <paramref name="fallbackValue"/> instead.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue)
        {
            Action<DelegateResult<TResult>> doNothing = _ => { };
            return policyBuilder.Fallback(() => fallbackValue, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            Action<DelegateResult<TResult>> doNothing = _ => { };
            return policyBuilder.Fallback(fallbackAction, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, TResult> fallbackAction)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));

            Action<DelegateResult<TResult>> doNothing = _ => { };
            return policyBuilder.Fallback(fallbackAction, doNothing);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then returns <paramref name="fallbackValue"/>.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Action<DelegateResult<TResult>> onFallback)
        {
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackValue, (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult> fallbackAction, Action<DelegateResult<TResult>> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), (outcome, ctx) => onFallback(outcome));
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then returns <paramref name="fallbackValue"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackValue">The fallback <typeparamref name="TResult"/> value to provide.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, TResult fallbackValue, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackValue, onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallback take a Context input parameter.")]
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        [Obsolete("This overload is deprecated and will be removed in a future release. Prefer the overload in which both fallbackAction and onFallback take a Context input parameter.")]
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<Context, CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return policyBuilder.Fallback((outcome, ctx, ct) => fallbackAction(ctx, ct), onFallback);
        }

        /// <summary>
        /// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="fallbackAction">The fallback action.</param>
        /// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
        /// <exception cref="System.ArgumentNullException">fallbackAction</exception>
        /// <exception cref="System.ArgumentNullException">onFallback</exception>
        /// <returns>The policy instance.</returns>
        public static FallbackPolicy<TResult> Fallback<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>, Context> onFallback)
        {
            if (fallbackAction == null) throw new ArgumentNullException(nameof(fallbackAction));
            if (onFallback == null) throw new ArgumentNullException(nameof(onFallback));

            return new FallbackPolicy<TResult>(
                (action, context, cancellationToken) => FallbackEngine.Implementation<TResult>(
                    action,
                    context,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates,
                    policyBuilder.ResultPredicates,
                    onFallback,
                    fallbackAction),
                policyBuilder.ExceptionPredicates,
                policyBuilder.ResultPredicates);
        }
    }
}
