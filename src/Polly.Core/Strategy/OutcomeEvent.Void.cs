using System;
using Polly.Strategy;

namespace Polly.Strategy;

public sealed partial class OutcomeEvent<TArgs>
    where TArgs : IResilienceArguments
{
    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> RegisterVoid(Action callback)
    {
        Guard.NotNull(callback);

        return ConfigureVoidCallbacks(c => c.Register(callback));
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> RegisterVoid(Action<Outcome> callback)
    {
        Guard.NotNull(callback);

        return ConfigureVoidCallbacks(c => c.Register(callback));
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> RegisterVoid(Action<Outcome, TArgs> callback)
    {
        Guard.NotNull(callback);

        return ConfigureVoidCallbacks(c => c.Register(callback));
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> RegisterVoid(Func<Outcome, TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        return ConfigureVoidCallbacks(c => c.Register(callback));
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="configure">Action that configures the callbacks.</param>
    /// <returns>The current updated instance.</returns>
    private OutcomeEvent<TArgs> ConfigureVoidCallbacks(Action<VoidOutcomeEvent<TArgs>> configure)
    {
        Guard.NotNull(configure);

        if (!_callbacks.TryGetValue(typeof(VoidResult), out var callbacks))
        {
            SetVoidCallbacks(new VoidOutcomeEvent<TArgs>());
            callbacks = _callbacks[typeof(VoidResult)];
        }

        configure((VoidOutcomeEvent<TArgs>)callbacks.callback);
        return this;
    }

    /// <summary>
    /// Sets callbacks for void-based results.
    /// </summary>
    /// <param name="callbacks">The callbacks instance.</param>
    /// <returns>The current updated instance.</returns>
    /// <remarks>
    /// This method replaces all previously registered callbacks for void-based results.
    /// </remarks>
    public OutcomeEvent<TArgs> SetVoidCallbacks(VoidOutcomeEvent<TArgs> callbacks)
    {
        Guard.NotNull(callbacks);

        _callbacks[typeof(VoidResult)] = (callbacks, callbacks.CreateHandler);
        return this;
    }
}
