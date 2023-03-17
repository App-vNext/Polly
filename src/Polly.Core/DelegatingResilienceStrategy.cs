namespace Polly;

/// <summary>
/// A resilience strategy that delegates the execution to the next strategy in the chain.
/// </summary>
public class DelegatingResilienceStrategy : IResilienceStrategy
{
    private bool _frozen;
    private IResilienceStrategy _next = NullResilienceStrategy.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingResilienceStrategy"/> class.
    /// </summary>
    protected DelegatingResilienceStrategy()
    {
    }

    /// <summary>
    /// Gets or sets the next resilience strategy in the chain.
    /// </summary>
    /// <remarks>This property cannot be changed once the strategy is executed.</remarks>
    public IResilienceStrategy Next
    {
        get => _next;
        set
        {
            Guard.NotNull(value);

            if (_frozen)
            {
                throw new InvalidOperationException($"The delegating resilience strategy is already frozen and changing the value of '{nameof(Next)}' property is not allowed.");
            }

            _next = value;
        }
    }

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable S4039 // Interface methods should be callable by derived types
    /// <inheritdoc/>
    ValueTask<TResult> IResilienceStrategy.ExecuteInternalAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        context.AssertInitialized();

        return ExecuteCoreAsync(callback, context, state);
    }
#pragma warning restore CA1033 // Interface methods should be callable by child types
#pragma warning restore S4039 // Interface methods should be callable by derived types

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    protected virtual ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        _frozen = true;
        return Next.ExecuteInternalAsync(callback, context, state);
    }

    internal void Freeze() => _frozen = true;
}

