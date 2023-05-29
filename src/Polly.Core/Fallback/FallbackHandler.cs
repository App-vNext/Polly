using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents a class for managing fallback handlers.
/// </summary>
internal sealed partial class FallbackHandler
{
    private readonly Dictionary<Type, object> _handlers = new();

    /// <summary>
    /// Gets a value indicating whether the fallback handler is empty.
    /// </summary>
    internal bool IsEmpty => _handlers.Count == 0;

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

        _handlers[typeof(TResult)] = handler!;

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

        _handlers[typeof(VoidResult)] = CreateGenericHandler(handler);

        return this;
    }

    internal Handler CreateHandler() => new(_handlers.ToDictionary(pair => pair.Key, pair => pair.Value));

    private static FallbackHandler<VoidResult> CreateGenericHandler(VoidFallbackHandler handler)
    {
        return new()
        {
            FallbackAction = async (outcome, args) =>
            {
                await handler.FallbackAction!(outcome.AsObjectOutcome(), args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
                return VoidResult.Instance;
            },
            ShouldHandle = (outcome, args) => handler.ShouldHandle!(outcome.AsObjectOutcome(), args)
        };
    }
}

