using System.Threading;

namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResilienceStrategy
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<ResilienceContext, TState> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        Strategy.Execute(
            static (context, state) =>
            {
                state.callback(context, state.state);
                return VoidResult.Instance;
            },
            context,
            (callback, state));
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Execute(
        Action<ResilienceContext> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        Strategy.Execute(
            static (context, state) =>
            {
                state(context);
                return VoidResult.Instance;
            },
            context,
            callback);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<TState, CancellationToken> callback,
        TState state,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        Strategy.Execute(
            static (state, token) =>
            {
                state.callback(state.state, token);
                return VoidResult.Instance;
            },
            (callback, state),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute(
        Action<CancellationToken> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        Strategy.Execute(
            static (state, token) =>
            {
                state(token);
                return VoidResult.Instance;
            },
            callback,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<TState> callback,
        TState state)
    {
        Guard.NotNull(callback);

        Strategy.Execute(
            static state =>
            {
                state.callback(state.state);
                return VoidResult.Instance;
            },
            (callback, state));
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute(Action callback)
    {
        Guard.NotNull(callback);

        Strategy.Execute(
            static state =>
            {
                state();
                return VoidResult.Instance;
            },
            callback);
    }
}
