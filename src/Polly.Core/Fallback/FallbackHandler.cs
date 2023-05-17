using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents a class for managing fallback handlers.
/// </summary>
public sealed partial class FallbackHandler
{
    private readonly OutcomePredicate<HandleFallbackArguments> _predicates = new();
    private readonly Dictionary<Type, object> _actions = new();

    /// <summary>
    /// Gets a value indicating whether the fallback handler is empty.
    /// </summary>
    internal bool IsEmpty => _predicates.IsEmpty;

    /// <summary>
    /// Configures a fallback handler for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="configure">An action that configures the fallback handler instance for a specific result.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the <see cref="FallbackHandler{TResult}"/> configured by <paramref name="configure"/> is invalid.</exception>
    public FallbackHandler SetFallback<TResult>(Action<FallbackHandler<TResult>> configure)
    {
        Guard.NotNull(configure);

        var handler = new FallbackHandler<TResult>();
        configure(handler);

        ValidationHelper.ValidateObject(handler, "The fallback handler configuration is invalid.");

        if (handler.ShouldHandle.IsEmpty)
        {
            return this;
        }

        _predicates.SetPredicates(handler.ShouldHandle);
        _actions[typeof(TResult)] = handler.FallbackAction!;

        return this;
    }

    /// <summary>
    /// Configures a void-based fallback handler.
    /// </summary>
    /// <param name="configure">An action that configures the void-based fallback handler.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the <see cref="VoidFallbackHandler"/> configured by <paramref name="configure"/> is invalid.</exception>
    public FallbackHandler SetVoidFallback(Action<VoidFallbackHandler> configure)
    {
        Guard.NotNull(configure);

        var handler = new VoidFallbackHandler();
        configure(handler);

        ValidationHelper.ValidateObject(handler, "The fallback handler configuration is invalid.");

        if (handler.ShouldHandle.IsEmpty)
        {
            return this;
        }

        _predicates.SetVoidPredicates(handler.ShouldHandle);
        _actions[typeof(VoidResult)] = CreateGenericAction(handler.FallbackAction!);

        return this;
    }

    internal Handler? CreateHandler()
    {
        var shouldHandle = _predicates.CreateHandler();
        if (shouldHandle == null)
        {
            return null;
        }

        return new Handler(shouldHandle, _actions);
    }

    private static Func<Outcome<VoidResult>, HandleFallbackArguments, ValueTask<VoidResult>> CreateGenericAction(Func<Outcome, HandleFallbackArguments, ValueTask> action)
    {
        return async (outcome, args) =>
        {
            await action(outcome.AsOutcome(), args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            return VoidResult.Instance;
        };
    }
}

