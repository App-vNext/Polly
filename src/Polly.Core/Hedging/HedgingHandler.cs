using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Represents a class for managing hedging handlers.
/// </summary>
public sealed partial class HedgingHandler
{
    private readonly OutcomePredicate<HandleHedgingArguments> _predicates = new();
    private readonly Dictionary<Type, object> _actions = new();

    /// <summary>
    /// Gets a value indicating whether the hedging handler is empty.
    /// </summary>
    internal bool IsEmpty => _actions.Count == 0;

    /// <summary>
    /// Configures a hedging handler for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="configure">An action that configures the hedging handler instance for a specific result.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the <see cref="HedgingHandler{TResult}"/> configured by <paramref name="configure"/> is invalid.</exception>
    public HedgingHandler SetHedging<TResult>(Action<HedgingHandler<TResult>> configure)
    {
        Guard.NotNull(configure);

        var handler = new HedgingHandler<TResult>();
        configure(handler);

        ValidationHelper.ValidateObject(handler, "The hedging handler configuration is invalid.");

        _predicates.SetPredicates(handler.ShouldHandle);
        _actions[typeof(TResult)] = handler.HedgingActionGenerator!;

        return this;
    }

    /// <summary>
    /// Configures a void-based hedging handler.
    /// </summary>
    /// <param name="configure">An action that configures the void-based hedging handler.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the <see cref="VoidHedgingHandler"/> configured by <paramref name="configure"/> is invalid.</exception>
    public HedgingHandler SetVoidHedging(Action<VoidHedgingHandler> configure)
    {
        Guard.NotNull(configure);

        var handler = new VoidHedgingHandler();
        configure(handler);

        ValidationHelper.ValidateObject(handler, "The hedging handler configuration is invalid.");

        _predicates.SetVoidPredicates(handler.ShouldHandle);
        _actions[typeof(VoidResult)] = CreateGenericGenerator(handler.HedgingActionGenerator!);

        return this;
    }

    internal Handler? CreateHandler()
    {
        if (_actions.Count == 0)
        {
            return null;
        }

        return new Handler(_predicates.CreateHandler(), _actions);
    }

    private static Func<HedgingActionGeneratorArguments<VoidResult>, Func<Task<VoidResult>>?> CreateGenericGenerator(
        Func<HedgingActionGeneratorArguments, Func<Task>?> generator)
    {
        return (args) =>
        {
            Func<Task>? action = generator(new HedgingActionGeneratorArguments(args.Context, args.Attempt));
            if (action == null)
            {
                return null;
            }

            return async () =>
            {
                await action().ConfigureAwait(args.Context.ContinueOnCapturedContext);
                return VoidResult.Instance;
            };
        };
    }
}

