namespace Polly;

/// <summary>
/// A resilience strategy that delegates the execution to the next strategy in the chain.
/// </summary>
public class DelegatingResilienceStrategy : IResilienceStrategy
{
    private bool _executed;
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

            if (_executed)
            {
                throw new InvalidOperationException("The delegating resilience strategy has already been executed and changing the value of 'Next' property is not allowed.");
            }

            _next = value;
        }
    }

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable S4039 // Interface methods should be callable by derived types
    /// <inheritdoc/>
    ValueTask<TResult> IResilienceStrategy.ExecuteInternalAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> execution,
        ResilienceContext context,
        TState state)
    {
        context.AssertInitialized();

        return ExecuteCoreAsync(execution, context, state);
    }
#pragma warning restore CA1033 // Interface methods should be callable by child types
#pragma warning restore S4039 // Interface methods should be callable by derived types

    /// <summary>
    /// Executes the <paramref name="execution"/> callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the execution.</typeparam>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents an asynchronous execution.</returns>
    protected virtual ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> execution,
        ResilienceContext context,
        TState state)
    {
        _executed = true;
        return Next.ExecuteInternalAsync(execution, context, state);
    }
}

